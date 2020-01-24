using System.Collections.Generic;
using System.Text;

namespace SyntacticAnalysis
{
    public class Rule
    {
        public readonly Symbol Key;
        public readonly List<Symbol> Symbols;
        public readonly string TextRepresentation;

        public Rule(Symbol key, string line, SyntacticConfigurationFile config) {
            this.Key = key;
            this.Symbols = new List<Symbol>();
            var sb = new StringBuilder();

            var symbols = line.Split(' ');
            foreach (var symbolData in symbols) {
                var symbol = new Symbol(symbolData, config);
                this.Symbols.Add(symbol);

                sb.Append(symbol.ID);
            }

            this.TextRepresentation = sb.ToString();
        }

        public Symbol? SymbolAfter(int i) {
            return i < this.Symbols.Count ? this.Symbols[i] : null;
        }

        public string ToStringWithSymbol(int ptr) {
            var sb = new StringBuilder();
            for (int i = 0; i <= this.Symbols.Count; i++) {
                if (i == ptr) {
                    sb.Append("|");
                }
                if (i < this.Symbols.Count) {
                    sb.Append(this.Symbols[i].ID);
                }
            }
            return sb.ToString();
        }

        public string ToString(int firstNSymbols) {
            var sb = new StringBuilder();
            for (int i = 0; i < this.Symbols.Count && i < firstNSymbols; i++) {
                sb.Append(this.Symbols[i].ID);
            }
            return sb.ToString();
        }

        public override bool Equals(object? obj) {
            if (obj is Rule r) {
                return this == r || this.Key.Equals(r.Key) && this.TextRepresentation == r.TextRepresentation;
            }
            return false;
        }

        public override int GetHashCode() {
            return this.Key.GetHashCode() + this.TextRepresentation.GetHashCode();
        }
    }
}
