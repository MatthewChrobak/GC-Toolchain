using Automata;
using Automata.NonDeterministic;
using Core;
using Core.ReportGeneration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LexicalAnalysis
{
    public class TokenParser
    {
        private NFATable nfaTable;
        private TokenStream stream;

        public TokenParser(NFATable nFATable) {
            this.nfaTable = nFATable;
        }

        private TokenStream Parse(TextFileScanner fs) {
            stream = new TokenStream();
            var tokenContent = new StringBuilder();
            int startLine = 0;
            int startColumn = 0;

            while (fs.CanRead && fs.Peek() != TextFileScanner.EOF) {
                tokenContent.Length = 0;

                startLine = fs.Line;
                startColumn = fs.Column;

                Token? token = null;
                var currentStates = this.nfaTable.ComputeNodeEClosure(this.nfaTable.StartState);

                do {
                    string symbol = fs.Peek().ToString();

                    if (symbol == TextFileScanner.EOF.ToString()) {
                        break;
                    }

                    tokenContent.Append(symbol);
                    currentStates = this.nfaTable.ApplyTransition(currentStates, symbol);

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

                Debug.Assert(token != null, $"Invalid symbol found during token parsing: '{tokenContent.ToString().Last()}' Ascii value:{(int)tokenContent.ToString().Last()} around {startLine}:{startColumn}");

                int goBackN = tokenContent.Length - token.Content.Length;
                for (int i = 0; i < goBackN; i++) {
                    fs.GoPrevious();
                }

                stream.Add(token);
            }

            return new TokenStream(stream);
        }

        public TokenStream ParseFile(string sourcefile) {
            var fs = new TextFileScanner(sourcefile);
            return Parse(fs);
        }

        public TokenStream ParseString(string line) {
            return this.ParseString(new string[] { line });
        }

        public TokenStream ParseString(string[] lines) {
            return this.Parse(new TextFileScanner(lines));
        }

        public ReportSection GetReportSections() {
            return new TokenStreamReportSection("Unfiltered Token Stream", this.stream);
        }
    }
}
