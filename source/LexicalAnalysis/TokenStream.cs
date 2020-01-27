using Core;
using System;
using System.Collections;
using System.Collections.Generic;

namespace LexicalAnalysis
{
    public class TokenStream : IEnumerable<Token>
    {
        private List<Token> _tokens;
        public int Count => this._tokens.Count;

        private int _ptr = -1;
        public bool HasPrevious => this._ptr - 1 >= 0;
        public bool HasNext => this._ptr + 1 < this.Count;
        public Token Current => this._tokens[this._ptr];
        public Token Next => this._tokens[++this._ptr];
        public Token Previous => this._tokens[--this._ptr];

        public void GoToStart() => this._ptr = -1;
        public void GoToEnd() => this._ptr = this.Count;

        public IEnumerable<Token> GetNextFewTokens(int count) {
            for (int i = 0; i < count; i++) {
                int ptr = this._ptr + i + 1;
                if (ptr < this._tokens.Count) {
                    yield return this._tokens[i];
                }
            }
        }

        public void RemoveAll(string tokenType) {
            int before = this._tokens.Count;
            this._tokens.RemoveAll(token => token.TokenType == tokenType);
            int after = this._tokens.Count;
            int totalRemoved = before - after;
            Log.WriteLineVerbose($"Removed {totalRemoved} tokens of type {tokenType} as specified by the blacklist of the syntax configuration file.");
        }

        public TokenStream() {
            this._tokens = new List<Token>();
        }

        public IEnumerator<Token> GetEnumerator() {
            return this._tokens.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this._tokens.GetEnumerator();
        }

        public void Add(Token token) {
            this._tokens.Add(token);
        }
    }
}
