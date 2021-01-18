using Core;
using Core.Logging;
using Core.ReportGeneration;
using GCT.Caching;
using GCT.Plugins;
using GCTPlugin;
using LexicalAnalysis;

namespace GCT
{
    public class CompilationProcess
    {
        private readonly Log? _log;
        private readonly Report? _report;

        private const string LexicalConfigurationFilePath = "tokens";
        private const string NoCacheFlag = "nocache";
        private const string LexerFlag = "lexer";

        public CompilationProcess(Log? log, Report? report) {
            this._log = log;
            this._report = report;
        }

        public void Start(ExecutionParameters parameters) {
            var pluginLoader = new PluginLoader(parameters);

            ComponentCache? cache = default;
            if (parameters.Get(NoCacheFlag, false) == false) {
                cache = new ComponentCache(parameters);
            }

            // Lexical analysis
            string? pluginId = parameters.Get(LexerFlag, null);
            var plugin = pluginLoader.Load(pluginId, this._log);
            this.LexicalAnalysis(plugin!, cache, parameters);
        }

        private void LexicalAnalysis(Plugin plugin, ComponentCache? cache, ExecutionParameters parameters) {
            string lexicalConfigurationFilePath = parameters.GetFilePath(LexicalConfigurationFilePath, "tokens");
            this._log?.WriteLineVerbose($"Parsing configuration file: {lexicalConfigurationFilePath}");
            var config = new LexicalConfigurationFile(lexicalConfigurationFilePath, this._log);
            this._log?.WriteLineVerbose($"Done.");

            ITokenParser? parser = default;

            if (cache is not null) {
                if (cache.IsCached(config, out string cachedFilePath)) {
                    parser = plugin.Deserialize<ITokenParser>(cachedFilePath) as ITokenParser;
                }
            }

            // Failed to load? Or just no cached version found?
            if (parser is null) {
                this._log?.WriteLineVerbose("No cached parser found. Constructing new parser");
                parser = plugin.Create<ITokenParser>(config);
            }

            Debug.Assert(parser != null, "Unable to construct a parser");
        }
    }
}
