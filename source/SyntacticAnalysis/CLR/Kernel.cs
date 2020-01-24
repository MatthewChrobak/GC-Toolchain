using System.Collections.Generic;

namespace SyntacticAnalysis.CLR
{
    public class Kernel
    {
        public readonly HashSet<ItemSet> ItemSets;

        public Kernel(ItemSet itemset) {
            this.ItemSets = new HashSet<ItemSet>();
            this.ItemSets.Add(itemset);
        }

        public Kernel(IEnumerable<ItemSet> itemsets) {
            this.ItemSets = new HashSet<ItemSet>(itemsets);
        }

        public override bool Equals(object? obj) {
            if (obj is Kernel k) {
                return this.ItemSets.HasSameElementsAs(k.ItemSets);
            }
            return false;
        }

        public override int GetHashCode() {
            return this.ItemSets.Count;
        }
    }
}
