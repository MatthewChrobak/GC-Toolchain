using System.Collections.Generic;

namespace SyntacticAnalysis.CLR
{
    public class ItemSet
    {
        public readonly Rule Rule;
        public readonly int Ptr;
        public readonly HashSet<Symbol> Lookahead;
        public Symbol? SymbolAfter => this.Rule.SymbolAfter(this.Ptr);
        public Symbol? SymbolAfterAfter => this.Rule.SymbolAfter(this.Ptr + 1);

        public ItemSet(Rule rule, int ptr) : this(rule, ptr, new Symbol[] { Symbol.EndStream }) {

        }

        public ItemSet(Rule rule, int ptr, Symbol lookahead) : this(rule, ptr, new Symbol[] { lookahead }) {

        }

        public ItemSet(Rule rule, int ptr, IEnumerable<Symbol> lookahead) {
            this.Rule = rule;
            this.Ptr = ptr;
            this.Lookahead = new HashSet<Symbol>(lookahead);
        }

        public override bool Equals(object? obj) {
            if (obj is ItemSet itemset) {

                if (this.Rule.Key != itemset.Rule.Key) {
                    return false;
                }

                if (this.Rule.ToStringWithSymbol(this.Ptr) != itemset.Rule.ToStringWithSymbol(itemset.Ptr)) {
                    return false;
                }

                return this.Lookahead.HasSameElementsAs(itemset.Lookahead);
            }
            return false;
        }

        public override int GetHashCode() {
            return this.Rule.TextRepresentation.GetHashCode() + this.Lookahead.Count;
        }
    }
}
