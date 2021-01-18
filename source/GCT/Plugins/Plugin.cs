﻿using Core;
using Core.Config;
using Core.Logging;
using GCTPlugin.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GCT.Plugins
{
    public class Plugin
    {
        public string Name => this._assembly.GetName().FullName;
        private readonly Assembly _assembly;
        private readonly Log? _log;

        public Plugin(string pluginPath, Log? log) {
            this._assembly = Assembly.LoadFrom(pluginPath);
            this._log = log;
        }

        private Type? FindType<T>() {
            var types = this._assembly.GetTypes().Where(type => type.IsInstanceOfType(typeof(T)) && type.IsInstanceOfType(typeof(IJsonSerializable)));
            if (types.Count() == 1) {
                return types.First();
            }
            else if (types.Count() == 0) {
                this._log?.WriteLineError($"Unable to find any instances of {typeof(T)} in {this._assembly.FullName} that also extends {typeof(IJsonSerializable)}");
                return null;
            }
            else {
                var type = types.First();
                this._log?.WriteLineWarning($"Found multiple instances of {typeof(T)} in {this._assembly.FullName}. Using {type.FullName}");
                return type;
            }
        }

        public T? Deserialize<T>(string cachedFilePath, ExecutionParameters executionParameters) where T : class {
            if (this.FindType<T>() is Type type) {
                foreach (var constructor in type.GetConstructors()) {
                    var parameters = constructor.GetParameters();
                    if (parameters.Length == 2 && parameters[0].ParameterType == typeof(JObject) && parameters[1].ParameterType == typeof(ExecutionParameters)) {
                        var jobject = JObject.Parse(File.ReadAllText(cachedFilePath));
                        var instance = Activator.CreateInstance(type, jobject);
                        return instance as T;
                    }
                }
                this._log?.WriteLineError($"Unable to find a constructor for type {type.FullName} in {this._assembly.FullName} with a constructor that takes in ({typeof(JObject)}, {typeof(ExecutionParameters)}");
            }
            return null;
        }

        public T? Create<T>(ConfigurationFile config, ExecutionParameters executionParameters) where T : class {
            if (this.FindType<T>() is Type type) {
                foreach (var constructor in type.GetConstructors()) {
                    var parameters = constructor.GetParameters();
                    if (parameters.Length == 2 && parameters[0].ParameterType == config.GetType() && parameters[1].ParameterType == typeof(ExecutionParameters)) {
                        var instance = Activator.CreateInstance(type, config);
                        return instance as T;
                    }
                }
                this._log?.WriteLineError($"Unable to find a constructor for type {type.FullName} in {this._assembly.FullName} with a constructor that takes in ({config.GetType()}, {typeof(ExecutionParameters)})");
            }
            return null;
        }
    }
}