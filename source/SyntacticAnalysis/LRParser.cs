using ASTVisitor;
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
        private readonly SyntacticConfigurationFile _config;

        private static Token EndStream => new Token(Symbol.EndStream.ID, "", 0, 0);

        public LRParser(SyntacticConfigurationFile config, TokenStream tokenstream) {
            this._parsingTrace = new List<(string stack, string nextInputs, string action)>();
            this._config = config;

            foreach (var section in config.GetSections(SyntacticConfigurationFile.SECTION_TAG_BLACKLIST)) {
                foreach (var line in section.Body) {
                    string tokenType = line.Trim();

                    if (!string.IsNullOrEmpty(tokenType)) {
                        tokenstream.RemoveAll(tokenType);
                    }
                }
            }
        }

        public ASTNode? Parse(LRParsingTable table, TokenStream tokenStream) {
            var stk = new Stack<StackElement>();
            stk.Push(new StateStackElement(0));
            var a = GetNextToken(tokenStream);

            while (true) {
                var x = stk.Peek() as StateStackElement;
                Debug.Assert(x != null, $"Expected {nameof(StateStackElement)} on the stack.");
                string parseAction = "Invalid";
                string stackContents = GetStackContent(stk);
                string lookaheadTokens = this.GetTokensForTable(tokenStream);
                var tableRow = table.Rows[x];

                if (tableRow.Actions.TryGetValue(a.TokenType, out var action)) {
                    if (action.Type == ActionType.Accept) {
                        parseAction = "accept";
                    } else if (action.Type == ActionType.Shift) {
                        stk.Push(new ASTNodeStackElement(a));
                        stk.Push(new StateStackElement(action.ID));
                        a = GetNextToken(tokenStream);
                        parseAction = $"Shift {action.ID}";
                    } else if (action.Type == ActionType.Reduce) {
                        var rule = table.Productions[action.ID];
                        int ruleCount = rule.Symbols.Count;

                        var node = new ASTNode();

                        for (int i = ruleCount - 1; i >= 0; i--) {
                            var id = stk.Pop();
                            var token = stk.Pop() as ASTNodeStackElement;
                            string? tag = rule.Symbols[i].Tag;
                            int number = node.Length;
                            if (tag != null) {

                                if (tag == this._config.GetRule(SyntacticConfigurationFile.RULE_INLINE_KEY).ToString()) {
                                    foreach (var element in token.ASTNode.Elements) {
                                        if (element.Value is ASTNode astNode) {
                                            node.Add(element.Key, astNode);
                                        }
                                        if (element.Value is List<ASTNode> lst) {
                                            foreach (var lstItem in lst) {
                                                node.Add(element.Key, lstItem);
                                            }
                                        }
                                    }
                                } else {
                                    node.Add(tag, token.ASTNode);
                                }
                            }
                        }
                        int nextState = stk.Peek() as StateStackElement;
                        stk.Push(new ASTNodeStackElement(node, rule.Key.ID));
                        stk.Push(new StateStackElement(table.Rows[nextState].GOTO[rule.Key.ID]));
                        parseAction = $"Reduce ({rule.Key.ID + " -> " + rule.TextRepresentation})";
                    }

                    this._parsingTrace.Add((stackContents, lookaheadTokens, parseAction));

                    if (parseAction == "accept") {
                        stk.Pop();
                        var element = stk.Pop() as ASTNodeStackElement;
                        return element?.ASTNode;
                    }
                } else {
                    Log.WriteLineVerboseClean($"couldn't retrieve: state {x} symbol {a.TokenType}");
                    Log.WriteLineError($"Expected {string.Join(", ", tableRow.Actions.Keys)}. Instead got {a.TokenType}");
                    return null;
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

        private string GetStackContent(Stack<StackElement> stk) {
            var arr = stk.ToArray();
            Array.Reverse(arr);

            int count = 10;
            if (arr.Length > count) {
                return "..." + string.Join("", arr[^10..].Select(val => val.ToString()));
            }
            return string.Join("", arr.Select(val => val.ToString()));
        }

        private Token GetNextToken(TokenStream tokenStream) {
            return tokenStream.HasNext ? tokenStream.Next : EndStream;
        }

        public ReportSection GetReportSection() {
            return new LRParseTraceReportSection(this._parsingTrace);
        }
    }
}
