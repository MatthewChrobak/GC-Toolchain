using Core;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using SyntacticAnalysis;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace SemanticAnalysis
{
    public class SemanticVisitor
    {
        private readonly string _pythonPluginPath;
        private readonly ScriptEngine _engine;
        private readonly ScriptScope _scope;
        private readonly ScriptSource _source;
        private readonly CompiledCode _compiled;

        public SemanticVisitor(string pythonPluginPath) {
            Log.WriteLineVerbose($"Creating visitor for {this._pythonPluginPath}.");

            this._pythonPluginPath = pythonPluginPath;
            this._engine = Python.CreateEngine();
            dynamic scope = this._engine.CreateScope();
            scope.log = CreateLogProxy();
            this._scope = scope;
            this._source = _engine.CreateScriptSourceFromFile(pythonPluginPath);
            this._compiled = this._source.Compile();

            this._compiled.Execute(this._scope);
        }

        private dynamic CreateLogProxy() {
            dynamic proxy = new ExpandoObject();
            proxy.WriteLineVerbose = new Action<string>(Log.WriteLineVerbose);
            proxy.WriteLineError = new Action<string>(Log.WriteLineError);
            return proxy;
        }

        public void Traverse(ASTNode ast) {
            Log.WriteLineVerbose($"Running visitor for {this._pythonPluginPath}.");
            var stk = new Stack<(string key, ASTNode node)>();
            stk.Push(("global", ast));

            while (stk.Count != 0) {
                (string key, var node) = stk.Pop();

                string id = $"visit_{key}";
                if (this._scope.TryGetVariable(id, out var variable)) {
                    this._engine.Operations.Invoke(variable, node);
                } else {
                    Log.WriteLineWarning($"Could not find handler for {id} in {this._pythonPluginPath}.");
                }

                foreach (var element in node.Elements) {
                    if (element.Value is List<ASTNode> lst) {
                        foreach (var member in lst) {
                            stk.Push((element.Key, member));
                        }
                    } else if (element.Value is ASTNode astnode) {
                        stk.Push((element.Key, astnode));
                    } else if (element.Value is string str) {
                        // Ignore
                    } else if (element.Value is int val) {
                        // Ignore
                    } else {
                        Log.WriteLineError($"Unknown element type in ASTNode: {element.Key}:{element.Value.GetType()}");
                    }
                }
            }
        }
    }
}
