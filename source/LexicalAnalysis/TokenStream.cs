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
