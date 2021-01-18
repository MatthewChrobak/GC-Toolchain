using Core;
using Core.LexicalAnalysis;
using Core.Logging;
using Core.ReportGeneration;
using GCT.Caching;
using GCT.Plugins;
using GCTPlugin;
using GCTPlugin.Serialization;

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

            var lexer = this.LexicalAnalysis(pluginLoader, cache, parameters);

            if (lexer is IJsonSerializable serializable) {
                cache?.Cache("test", serializable);
            }
        }

        private ILexicalAnalyzer LexicalAnalysis(PluginLoader pluginLoader, ComponentCache? cache, ExecutionParameters parameters) {

            // Get the plugin for lexical analysis
            string? pluginId = parameters.Get(LexerFlag, null);
            var plugin = pluginLoader.Load(pluginId, this._log);

            // Get the configuration file
            string lexicalConfigurationFilePath = parameters.GetFilePath(LexicalConfigurationFilePath, "tokens");
            this._log?.WriteLineVerbose($"Parsing configuration file: {lexicalConfigurationFilePath}");
            var config = new LexicalConfigurationFile(lexicalConfigurationFilePath, this._log);
            this._log?.WriteLineVerbose($"Done.");

            // Either deserialize the parser, or create a new one.
            ILexicalAnalyzer? parser = default;

            if (cache is not null) {
                if (cache.IsCached($"{plugin.Name}-{config.GetCacheID()}", out string cachedFilePath)) {
                    parser = plugin.Deserialize<ILexicalAnalyzer>(cachedFilePath, parameters);
                }
            }

            // Failed to load? Or just no cached version found?
            if (parser is null) {
                this._log?.WriteLineVerbose("No cached parser found. Constructing new parser");
                parser = plugin.Create<ILexicalAnalyzer>(config, parameters);
            }

            Debug.Assert(parser != null, "Unable to construct a parser");

            // Add stuff to the report
            if (parser is IReportable reportable) {
                this._report?.Add(reportable);
            }

            return parser!;
        }
    }
}
