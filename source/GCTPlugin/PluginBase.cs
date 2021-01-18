using Core;
using Core.Config;
using Core.Logging;
using GCTPlugin.Serialization;
using Newtonsoft.Json.Linq;

namespace GCTPlugin
{
    public abstract class PluginBase : IJsonSerializable
    {
        protected readonly Log? _log;

        public PluginBase(JObject json, ExecutionParameters executionParameters, Log? log) {
            this._log = log;
        }

        public PluginBase(ConfigurationFile config, ExecutionParameters executionParameters, Log? log) {
            this._log = log;
        }

        public abstract JObject GetJson();
        public abstract void SetFromJson(JObject node);
    }
}
