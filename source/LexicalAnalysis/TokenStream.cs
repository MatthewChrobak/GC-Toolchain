using System.Collections;
using System.Collections.Generic;

namespace LexicalAnalysis
{
    public class TokenStream : IEnumerable<Token>
    {
        private List<Token> _tokens;

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
