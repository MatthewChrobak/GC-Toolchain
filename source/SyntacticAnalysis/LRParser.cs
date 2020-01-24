using Core;
using LexicalAnalysis;
using System;
using System.Collections.Generic;

namespace SyntacticAnalysis
{
    public class LRParser
    {
        public bool Parse(LRParsingTable table, TokenStream tokenStream) {
            var stk = new Stack<dynamic>();
            stk.Push(0);
            var a = GetNextToken(tokenStream);

            while (true) {
                var x = stk.Peek();

                if (table.Rows[x].Actions.TryGetValue(a.TokenType, out var action)) {
                    if (action.Type == ActionType.Accept) {
                        Log.WriteLineVerboseClean("accept");
                        return true;
                    }
                    if (action.Type == ActionType.Shift) {
                        stk.Push(a.TokenType);
                        stk.Push(action.ID);
                        a = GetNextToken(tokenStream);
                        Log.WriteLineVerboseClean($"Shift {action.ID}");
                    } else if (action.Type == ActionType.Reduce) {
                        var prod = table.Productions[action.ID];
                        int prodCount = prod.   Symbols.Count;
                        for (int i = 0; i < 2 * prodCount; i++) {
                            // TODO: Is this where we construct the AST?
                            stk.Pop();
                        }
                        int nextState = stk.Peek();
                        stk.Push(prod.Key.ID);
                        stk.Push(table.Rows[nextState].GOTO[prod.Key.ID]);
                        Log.WriteLineVerboseClean($"Reduce ({prod.Key.ID + " -> " + prod.TextRepresentation})");
                    } else {
                        return false;
                    }
                } else {
                    Log.WriteLineVerboseClean($"couldn't retrieve");
                    Log.WriteLineVerboseClean($"state {x} symbol {a.TokenType}");
                    return false;
                }
            }
        }

        private Token GetNextToken(TokenStream tokenStream) {
            return tokenStream.HasNext ? tokenStream.Next : new Token(Symbol.EndStream.ID, "", 0, 0);
        }
    }
}
