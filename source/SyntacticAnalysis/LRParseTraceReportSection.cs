using Core.ReportGeneration;
using System.Collections.Generic;
using System.Text;

namespace SyntacticAnalysis
{
    public class LRParseTraceReportSection : ReportSection
    {
        private List<(string stack, string nextInputs, string action)> _parsingTrace;

        public LRParseTraceReportSection(List<(string stack, string nextInputs, string action)> parsingTrace) : base("LR-Parsing-Trace") {
            this._parsingTrace = parsingTrace;
        }

        public override string GetContent() {
            var sb = new StringBuilder();
            sb.Append(@"
<table class='table'>
    <thead>
        <tr>
            <th scope='col'>State</th>
            <th scope='col'>Stack</th>
            <th scope='col'>Input</th>
            <th scope='col'>Action</th>
        </tr>");

            for (int i = 0; i < this._parsingTrace.Count; i++) {
                var row = this._parsingTrace[i];
                sb.Append($"<tr>");
                sb.Append($"<td>{i}</td>");
                sb.Append($"<td>{row.stack}</td>");
                sb.Append($"<td>{row.nextInputs}</td>");
                sb.Append($"<td>{row.action}</td>");
                sb.Append($"</tr>");
            }

            sb.Append(@"
    </thead>
</table>
");
            return sb.ToString();
        }
    }
}