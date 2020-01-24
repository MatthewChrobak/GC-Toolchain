using System.Collections.Generic;
using System.Text;

namespace SyntacticAnalysis.CLR
{
    public class State
    {
        public ItemSet ItemSetID;
        public HashSet<ItemSet> ItemSets;

        public State(ItemSet itemSet) {
            this.ItemSetID = itemSet;
            this.ItemSets = new HashSet<ItemSet>();
            this.ItemSets.Add(this.ItemSetID);
        }

        public void AddItemSets(IEnumerable<ItemSet> itemSets) {
            foreach (var itemset in itemSets) {
                this.ItemSets.Add(itemset);
            }
        }

        public override string ToString() {
            var sb = new StringBuilder();

            foreach (var itemset in this.ItemSets) {
                sb.AppendLine(itemset.ToString());
            }

            return sb.ToString();
        }
    }
}