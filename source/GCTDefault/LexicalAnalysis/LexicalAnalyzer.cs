using Core;
using Core.LexicalAnalysis;
using Core.Logging;
using Core.ReportGeneration;
using GCTPlugin;
using GCTPlugin.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace GCTDefault.LexicalAnalysis
{
    public partial class LexicalAnalyzer : PluginBase
    {
        private TokenParser _parser;

        public LexicalAnalyzer(JObject json, ExecutionParameters executionParameters, Log? log) : base(json, executionParameters, log) {
            this._parser = new TokenParser(json, log);
            this.SetFromJson(json);
        }

        public LexicalAnalyzer(LexicalConfigurationFile config, ExecutionParameters executionParameters, Log? log) : base(config, executionParameters, log) {
            var generator = new TokenParserTableGenerator(config, log);
            this._parser = new TokenParser(generator.NFATable, log);
        }
    }

    public partial class LexicalAnalyzer : ILexicalAnalyzer
    {
        public TokenStream Parse(string programText) {
            return this._parser.Parse(programText);
        }
    }

    public partial class LexicalAnalyzer : IReportable
    {
        public IEnumerable<ReportSection> GetReportSections() {
            return Array.Empty<ReportSection>();
        }
    }

    public partial class LexicalAnalyzer : IJsonSerializable
    {
        public override JObject GetJson() {
            return this._parser.GetJson();
        }

        public override void SetFromJson(JObject node) {
            this._parser = new TokenParser(node, this._log);
        }
    }
}
