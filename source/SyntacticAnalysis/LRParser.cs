using Core;
using Core.ReportGeneration;
using LexicalAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyntacticAnalysis
{
    public class LRParser
    {
        private List<(string stack, string nextInputs, string action)> _parsingTrace;
        private static Token EndStream => new Token(Symbol.EndStream.ID, "", 0, 0);

        public LRParser(SyntacticConfigurationFile config, TokenStream tokenstream) {
            this._parsingTrace = new List<(string stack, string nextInputs, string action)>();
         
            foreach (var section in config.GetSections(SyntacticConfigurationFile.SECTION_TAG_BLACKLIST)) {
                foreach (var line in section.Body) {
                    string tokenType = line.Trim();

                    if (!string.IsNullOrEmpty(tokenType)) {
                        tokenstream.RemoveAll(tokenType);
                    }
                }
            }
        }

        public bool Parse(LRParsingTable table, TokenStream tokenStream) {
            var stk = new Stack<dynamic>();
            stk.Push(0);
            var a = GetNextToken(tokenStream);

            while (true) {
                var x = stk.Peek();
                string parseAction = "Invalid";
                string stackContents = GetStackContent(stk);
                string lookaheadTokens = this.GetTokensForTable(tokenStream);

                if (table.Rows[x].Actions.TryGetValue(a.TokenType, out var action)) {
                    if (action.Type == ActionType.Accept) {
                        parseAction = "accept";
                    } else if (action.Type == ActionType.Shift) {
                        stk.Push(a.TokenType);
                        stk.Push(action.ID);
                        a = GetNextToken(tokenStream);
                        parseAction = $"Shift {action.ID}";
                    } else if (action.Type == ActionType.Reduce) {
                        var prod = table.Productions[action.ID];
                        int prodCount = prod.Symbols.Count;
                        for (int i = 0; i < 2 * prodCount; i++) {
                            // TODO: Is this where we construct the AST?
                            stk.Pop();
                        }
                        int nextState = stk.Peek();
                        stk.Push(prod.Key.ID);
                        stk.Push(table.Rows[nextState].GOTO[prod.Key.ID]);
                        parseAction = $"Reduce ({prod.Key.ID + " -> " + prod.TextRepresentation})";
                    }

                    this._parsingTrace.Add((stackContents, lookaheadTokens, parseAction));

                    if (parseAction == "accept") {
                        return true;
                    }
                } else {
                    Log.WriteLineVerboseClean($"couldn't retrieve: state {x} symbol {a.TokenType}");
                    return false;
                }
            }
        }

        private string GetTokensForTable(TokenStream tokenStream) {
            int count = 10;
            var tokens = tokenStream.GetNextFewTokens(count).ToList();
            int actualCount = tokens.Count;
            if (tokens.Count < count) {
                tokens.Add(EndStream);
            }
            return string.Join("", tokens.Select(val => val.TokenType)) + (actualCount >= count ? "..." : "");
        }

        private string GetStackContent(Stack<dynamic> stk) {
            var arr = stk.ToArray();
            Array.Reverse(arr);

            int count = 10;
            if (arr.Length > count) {
                return "..." + string.Join("", arr[^10..]);
            }
            return string.Join("", arr);
        }

        private Token GetNextToken(TokenStream tokenStream) {
            return tokenStream.HasNext ? tokenStream.Next : EndStream;
        }

        public ReportSection GetReportSection() {
            return new LRParseTraceReportSection(this._parsingTrace);
        }
    }
}
