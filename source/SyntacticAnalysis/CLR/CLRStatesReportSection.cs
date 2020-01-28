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
    }
}