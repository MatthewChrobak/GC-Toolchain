using Automata;
using Automata.NonDeterministic;
using Core;
using Core.LexicalAnalysis;
using Core.Logging;
using Core.ReportGeneration;
using GCTDefault.LexicalAnalysis.ReportGeneration;
using GCTPlugin.Serialization;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCTDefault.LexicalAnalysis
{
    public partial class TokenParser
    {
        private NFATable _nfaTable;
        private Log? _log;
        private TokenStream? stream;

#pragma warning disable CS8618
        public TokenParser(JObject node, Log? log) {
            this._log = log;
            SetFromJson(node);
#pragma warning disable
        }

        public TokenParser(NFATable nFATable, Log? log) {
            this._nfaTable = nFATable;
            this._log = log;
        }

        private TokenStream Parse(TextFileScanner fs) {
            stream = new TokenStream(this._log);
            var tokenContent = new StringBuilder();
            int startLine = 0;
            int startColumn = 0;

            while (fs.CanRead && fs.Peek() != TextFileScanner.EOF) {
                tokenContent.Length = 0;

                startLine = fs.Line;
                startColumn = fs.Column;

                Token? token = null;
                var currentStates = this._nfaTable.ComputeNodeEClosure(this._nfaTable.StartState);

                do {
                    string symbol = fs.Peek().ToString();

                    if (symbol == TextFileScanner.EOF.ToString()) {
                        break;
                    }

                    tokenContent.Append(symbol);
                    currentStates = this._nfaTable.ApplyTransition(currentStates, symbol);

                    var finalNodes = currentStates.Where(node => node.IsFinal);
                    var tags = new HashSet<(int priority, string tag)>();
                    foreach (var node in finalNodes) {
                        foreach (var fullTag in node.GetTags(Node.LABEL)) {
                            var data = fullTag.Split('~');
                            string tag = data[0];
                            int priority = int.Parse(data[1]);
                            tags.Add((priority, tag));
                        }
                    }
                    var orderedTags = tags.OrderByDescending(val => val.priority);
                    var highestPriority = orderedTags.FirstOrDefault();
                    var secondHighestPriority = orderedTags.ElementAtOrDefault(1);
                    string highestPriorityTag = highestPriority.tag;
                    if (highestPriorityTag != null) {
                        Debug.Assert(secondHighestPriority.tag == null || highestPriority.priority > secondHighestPriority.priority, $"Conflict of priority found between {highestPriority.tag}:{highestPriority.priority} and {secondHighestPriority.tag}:{secondHighestPriority.priority}");
                        token = new Token(highestPriorityTag, tokenContent.ToString(), startLine + 1, startColumn + 1);
                    }
                    if (!fs.CanRead && fs.Peek() == TextFileScanner.EOF) {
                        break;
                    }
                    fs.GoNext();
                } while (currentStates.Count > 0);

                Debug.Assert(token != null, $"Invalid symbol found during token parsing: '{tokenContent.ToString().Last()}' Hex value:{((int)tokenContent.ToString().Last()).ToString("X")} around {startLine}:{startColumn}");

                int goBackN = tokenContent.Length - token!.Content.Length;
                for (int i = 0; i < goBackN; i++) {
                    fs.GoPrevious();
                }

                stream.Add(token);
            }

            return new TokenStream(stream, this._log);
        }

        public TokenStream Parse(string content) {
            return this.Parse(new TextFileScanner(content));
        }

        public TokenStream Parse(string[] lines) {
            return this.Parse(new TextFileScanner(lines));
        }

        public ReportSection GetReportSections() {
            return new TokenStreamReportSection("Unfiltered Token Stream", this.stream);
        }
    }

    public partial class TokenParser : IJsonSerializable
    {
        public JObject GetJson() {
            var json = new JObject();

            var nodesJson = new JArray();
            nodesJson["num-states"] = this._nfaTable.Nodes.Count();
            nodesJson["start-state"] = this._nfaTable.StartState.ID;
            foreach (var nodeNfa in this._nfaTable.Nodes) {
                var nodeJson = new JObject();
                nodeJson[nameof(nodeNfa.ID)] = nodeNfa.ID;
                nodeJson[nameof(nodeNfa.IsFinal)] = nodeNfa.IsFinal;

                var tagsArray = new JArray();
                foreach (var tag in nodeNfa.GetTags()) {
                    tagsArray.Add(tag);
                }
                nodeJson["Tags"] = tagsArray;
                
                nodesJson.Add(nodeJson);
            }

            var transitionsJson = new JArray();
            foreach (var transitionNfa in this._nfaTable.Transitions) {
                var transitionJson = new JObject();
                transitionJson[nameof(transitionNfa.Source)] = transitionNfa.Source.ID;
                transitionJson[nameof(transitionNfa.Destination)] = transitionNfa.Destination.ID;
                transitionJson[nameof(transitionNfa.Symbol)] = transitionNfa.Symbol;
                transitionsJson.Add(transitionJson);
            }

            json.Add(nameof(this._nfaTable.Nodes), nodesJson);
            json.Add(nameof(this._nfaTable.Transitions), transitionsJson);
            return json;
        }

        public void SetFromJson(JObject json) {
            this._nfaTable = new NFATable();

            if (json[nameof(this._nfaTable.Nodes)] is JArray nodesJson) {
                foreach (var nodeJson in nodesJson) {
                    int id = int.Parse(nodeJson[nameof(Node.ID)].ToString());
                    bool isFinal = bool.Parse(nodeJson[nameof(Node.IsFinal)].ToString());
                    var node = new Node(id, isFinal);

                    if (nodeJson["Tags"] is JArray tagsJson) {

                    }

                }
            }

            var transitions = json[nameof(this._nfaTable.Transitions)] as JArray;
        }
    }
}
