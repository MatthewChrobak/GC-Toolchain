using Automata;
using Automata.NonDeterministic;
using Core.ReportGeneration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LexicalAnalysis.ReportGeneration
{
    internal class AutomataVisualizationSection : ReportSection
    {
        private StringBuilder _edges;
        private StringBuilder _nodes;

        public AutomataVisualizationSection(string id, AutomataTable table) : base(id) {
            this.ParseNodes(table);
            this.ParseEdges(table);
        }

        private void ParseEdges(AutomataTable table) {
            var transitions = new Dictionary<(Node a, Node b), HashSet<string>>();
            
            foreach (var transition in table.Transitions) {
                var a = transition.Source;
                var b = transition.Destination;
                transitions[(a, b)] = new HashSet<string>();
            }

            foreach (var transition in table.Transitions) {
                var a = transition.Source;
                var b = transition.Destination;

                string? symbol = transition.Symbol;
                if (symbol == NFATable.Epsilon) {
                    symbol = "ε";
                }
                transitions[(a, b)].Add(symbol);
            }

            this._edges = new StringBuilder();
            foreach (var entry in transitions) {
                var a = entry.Key.a;
                var b = entry.Key.b;

                var symbol = GetRangeEdgeValues(entry.Value);
                this._edges.Append(@$"{a.ID} -> {b.ID}[label=""{symbol}""]; ");
            }
        }

        private string GetRangeEdgeValues(HashSet<string> transitions) {
            var values = transitions.OrderBy(val => ToInt(val)).ToArray();
            var results = new List<string>();

            char? lb = null;
            char? ub = null;

            for (int i = 0; i < values.Length; i++) {
                string value = values[i];
                if (value.Length == 1) {
                    char c = value[0];
                    if (lb == null) {
                        lb = c;
                        ub = c;
                    }
                    if ((int)c - (int)ub <= 1) {
                        ub = c;
                    } else {
                        if (lb == ub) {
                            results.Add(EscapeSpecialCharacters(lb));
                        } else {
                            results.Add($"{EscapeSpecialCharacters(lb)}-{EscapeSpecialCharacters(ub)}");
                        }
                        lb = null;
                        ub = null;
                        i--;
                    }
                } else {
                    if (lb != null && ub != null) {
                        if (lb == ub) {
                            results.Add(EscapeSpecialCharacters(lb));
                        } else {
                            results.Add($"{EscapeSpecialCharacters(lb)}-{EscapeSpecialCharacters(ub)}");
                        }
                        lb = null;
                        ub = null;
                    }
                    results.Add(value);
                }
            }

            string? strlb = EscapeSpecialCharacters(lb);
            string? strub = EscapeSpecialCharacters(ub);

            if (strlb != null && strub != null) {
                if (strlb == strub) {
                    results.Add($"{strlb}");
                } else {
                    results.Add($"{strlb}-{strub}");
                }
            }

            return string.Join(", ", results).Replace("\"", "\\\"").Replace("'", "\\'");
        }

        private string EscapeSpecialCharacters(char? val) {
            if (val == null) {
                return null;
            }

            switch (val) {
                case '\r':
                    return "/r";
                case '\n':
                    return "/n";
                case '\t':
                    return "/t";
                case ' ':
                    return "' '";
                case '"':
                    return "''";
                default:
                    return val.ToString();
            }
        }

        private object ToInt(string val) {
            int sum = 0;
            foreach (var v in val) {
                sum += (int)v;
            }
            return sum;
        }

        private void ParseNodes(AutomataTable table) {
            this._nodes = new StringBuilder();

            foreach (var node in table.Nodes) {
                this._nodes.Append($"{node.ID}[");
                if (node.IsFinal) {
                    this._nodes.Append("color=yellow ");
                }
                string label = string.Join(", ", node.GetTags(Node.LABEL));
                if (label.Length == 0) {
                    label = node.ID.ToString();
                }
                this._nodes.Append(@$"label=""{label}""");
                this._nodes.Append("]; ");
            }
        }

        public override string GetContent() {
            string id = this.ID_HTML.Replace(' ', '_') + "_visjs";
            return $@"
<div id=""{id}"" style=""margin: auto; width: 1500; height: 750px; border: 1px solid gray;""></div>
<script type=""text/javascript"">
var json = '{this.GetData()}';
var data = vis.parseDOTNetwork(json);
var container = document.getElementById(""{id}"");
var options = {{}};
var network = new vis.Network(container, data, options);
</script>
";
        }

        private string GetData() {
            return $@"digraph {{ node[shape=circle color=lightgray] edge [color=black, fontcolor=black, length=200] {this._nodes.ToString()} {this._edges.ToString()} }}";
        }
    }
}