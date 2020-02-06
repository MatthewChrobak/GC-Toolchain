using Core.ReportGeneration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyntacticAnalysis
{
    public class SyntacticGrammarReportSection : ReportSection
    {
        private Dictionary<string, Production> _productions;

        public SyntacticGrammarReportSection(Dictionary<string, Production> productions) : base("Syntactic-Grammar") {
            this._productions = productions;
        }

        public override string GetContent() {
            var sb = new StringBuilder();

            foreach (var production in this._productions) {
                sb.Append("<p>");

                foreach (var rule in production.Value.Rules) {
                    sb.Append($"<div>{rule.Key.ID} -> {string.Join(' ', rule.Symbols.Select(symbol => symbol.ID))}</div>");
                }

                sb.Append("</p>");
            }

            return sb.ToString();
        }
    }
}