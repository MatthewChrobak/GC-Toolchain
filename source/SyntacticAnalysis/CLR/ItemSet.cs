using System.Collections.Generic;
using System.Linq;

namespace SyntacticAnalysis.CLR
{
    public class ItemSet
    {
        public readonly Rule Rule;
        public readonly int Ptr;
        //public readonly HashSet<Symbol> Follow;

        public ItemSet(Rule rule, int ptr/*, HashSet<Symbol> follow*/) {
            this.Rule = rule;
            this.Ptr = ptr;
            //this.Follow = follow;
        }

        public override string ToString() {
            return $"{this.Rule.Key.ID} -> {this.Rule.ToStringWithSymbol(this.Ptr)} : {""/*string.Join(',', this.Follow.Select(follow => follow.ID))*/}";
        }

        public Symbol? NextSymbol() {
            return this.Rule.SymbolAfter(this.Ptr);
        }

        public override bool Equals(object? obj) {
            if (obj is ItemSet itemset) {
                return this.Ptr == itemset.Ptr && this.Rule.Equals(itemset.Rule) /*&& this.Follow == itemset.Follow*/;
            }
            return false;
        }

        public override int GetHashCode() {
            return this.Rule.GetHashCode() + this.Ptr.GetHashCode();
        }
    }
}