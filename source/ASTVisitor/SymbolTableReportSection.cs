using Core.ReportGeneration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTVisitor
{
    internal class SymbolTableReportSection : ReportSection
    {
        private Dictionary<string, SymbolTable> globalScope;

        public SymbolTableReportSection(Dictionary<string, SymbolTable> symbolTables) : base("Symbol-Tables") {
            this.globalScope = symbolTables;
        }

        public override string GetContent() {
            var sb = new StringBuilder();

            foreach (var entry in this.globalScope.OrderBy(entry => entry.Key)) {
                var table = entry.Value;
                var symbols = new HashSet<string>();
                foreach (var row in table.Rows) {
                    foreach (var column in row.Elements) {
                        symbols.Add(column.Key);
                    }
                }

                sb.Append(@$"
<div>
    <h2>{entry.Key}</h2>
    <table class='table'>
        <thead>
            <tr>
");

                foreach (var symbol in symbols) {
                    sb.Append($"<th scope='col'>{symbol}</th>");
                }

                sb.Append(@"
            </tr>
        </thead>
        <tbody>
");
                foreach (var row in table.Rows) {
                    sb.Append("<tr>");
                    foreach (var symbol in symbols) {
                        string content = string.Empty;
                        if (row.Contains(symbol)) {
                            content = row[symbol].ToString();
                        }
                        sb.Append($"<td>{content}</td>");
                    }
                    sb.Append("</tr>");
                }
                sb.Append(@"
        </tbody>
    </table>
</div>");
            }

            return sb.ToString();
        }
    }
}