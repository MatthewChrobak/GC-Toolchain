using Core.ReportGeneration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyntacticAnalysis.CLR
{
    internal class CLRStatesReportSection : ReportSection
    {
        private Dictionary<Kernel, State> states;

        public CLRStatesReportSection(Dictionary<Kernel, State> states) : base("CLR-States") {
            this.states = states;
        }

        public override string GetContent() {
            var sb = new StringBuilder();
            sb.Append($"<h2>Num States: {this.states.Count}</h2>");
            foreach (var state in this.states) {
                sb.Append("<p>");
                sb.Append($"<h3>{state.Value.ID}</h3>");
                foreach (var itemset in state.Value.Closure) {
                    sb.Append($"<div>{ itemset.Rule.Key.ID} -> { itemset.Rule.ToStringWithSymbol(itemset.Ptr)}, { string.Join(',', itemset.Lookahead.Select(l => l.ID))}</div>");
                }
                sb.Append("</p>");
            }
            return sb.ToString();
        }

        public string ToTestString() {
            var sb = new StringBuilder();

            foreach (var state in this.states) {
                sb.AppendLine($"{{{string.Join("; ", state.Value.Closure.Select(val => fmt(val)))}}}");
            }

            return sb.ToString();
        }

        private string fmt(ItemSet val) {
            return $"[{val.Rule.Key.ID} -> {val.Rule.ToStringWithSymbol(val.Ptr, '.')}, {string.Join('/', val.Lookahead.Select(l => l.ID))}]";
        }
    }
}