using Core.ReportGeneration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyntacticAnalysis
{
    public class LRParsingTableReportSection : ReportSection
    {
        private string[] terminals;
        private string[] productions;
        private LRParsingTableRow[] rows;

        public LRParsingTableReportSection(LRParsingTable lRParsingTable) : base("LR-Parsing-Table") {
            var terminals = new HashSet<string>();
            var productions = new HashSet<string>();

            foreach (var row in lRParsingTable.Rows) {
                foreach (var gt in row.GOTO) {
                    productions.Add(gt.Key);
                }
                foreach (var action in row.Actions) {
                    terminals.Add(action.Key);
                }
            }

            this.terminals = terminals.ToArray();
            this.productions = productions.ToArray();
            this.rows = lRParsingTable.Rows;
        }

        public override string GetContent() {
            var sb = new StringBuilder();

            sb.Append(@"
<table class='table'>
    <thead>
        <tr>
            <th scope='col'>State</th>");

            foreach (var terminal in this.terminals) {
                sb.Append($"<th scope='col'>{terminal}</th>");
            }
            foreach (var production in this.productions) {
                sb.Append($"<th scope='col'>{production}</th>");
            }

            for (int i = 0; i < this.rows.Length; i++) {
                var row = this.rows[i];
                sb.Append($"<tr><th scope='row'>{i}</th>");

                foreach (var terminal in this.terminals) {
                    sb.Append($"<td>{row.GetAction(terminal)}</td>");
                }

                foreach (var production in this.productions) {
                    sb.Append($"<td>{row.GetGOTO(production)}</td>");
                }

                sb.Append($"<tr>");
            }

            sb.Append(@"
        </tr>
    </thead>
</table>
");

            return sb.ToString();
        }
    }
}