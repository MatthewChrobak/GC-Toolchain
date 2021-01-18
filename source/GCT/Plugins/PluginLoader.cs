using Core;
using Core.Logging;
using System.IO;

namespace GCT.Plugins
{
    public class PluginLoader
    {
        private const string PluginFolderPath = "p";
        private readonly string _pluginFolderPath;

        public PluginLoader(ExecutionParameters parameters) {
            this._pluginFolderPath = parameters.GetFolderPath(PluginFolderPath, "plugins");
        }

        public Plugin? Load(string? pluginId, Log? log) {
            string pluginPath = Path.Combine(this._pluginFolderPath, pluginId ?? "");
            if (File.Exists(pluginPath)) {
                return new Plugin(pluginPath, log);
            }
            Debug.Assert(false, $"Unable to find a plugin with the id {pluginId}");
            return null;
        }
    }
}
