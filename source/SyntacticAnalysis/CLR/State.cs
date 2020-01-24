using System.Collections.Generic;
using System.Linq;

namespace SyntacticAnalysis.CLR
{
    public class State
    {
        public readonly string ID;
        public readonly Kernel Kernel;
        public readonly HashSet<ItemSet> Closure;

        public State(string id, Kernel kernel) {
            this.ID = id;
            this.Kernel = kernel;
            this.Closure = new HashSet<ItemSet>(kernel.ItemSets);
        }

        public void ComputeClosure(ProductionTable productionTable) {
            var queue = new Queue<ItemSet>(this.Kernel.ItemSets);

            // We never have to process an item-set twice because we'll re-merge them later.
            // So queue them.
            while (queue.Count != 0) {
                var existingClosureItemSet = queue.Dequeue();
                var symbolAfter = existingClosureItemSet?.SymbolAfter;

                // If the symbol-after the PTR is a production, add its rules to the state with the lookahead of the symbol-after-after IE First(SymbolAfterAfter).
                // If it's not a production, we don't care.
                if (symbolAfter?.Type == SymbolType.Production) {

                    var production = productionTable.GetProduction(symbolAfter.ID);
                    foreach (var rule in production.Rules) {

                        // Generate the new itemset for the closure.
                        ItemSet newClosureItemSet;
                        var symbolAfterAfter = existingClosureItemSet.SymbolAfterAfter;

                        // If null, lookahead is the previous itemset lookahead.
                        if (symbolAfterAfter == null) {
                            newClosureItemSet = new ItemSet(rule, 0, existingClosureItemSet.Lookahead);

                            // If production, lookahead is its first
                        } else if (symbolAfterAfter.Type == SymbolType.Production) {
                            newClosureItemSet = new ItemSet(rule, 0, productionTable.GetFirst(symbolAfterAfter));
                        } else {
                            // If token, lookahead is token
                            newClosureItemSet = new ItemSet(rule, 0, symbolAfterAfter);
                        }

                        // Don't add if it already exists.
                        if (!this.Closure.Contains(newClosureItemSet)) {
                            this.Closure.Add(newClosureItemSet);
                            queue.Enqueue(newClosureItemSet);
                        }
                    }
                }
            }
        }

        public void MergeLookaheads() {
            var lookup = new Dictionary<(Rule, int), ItemSet>();
            var remove = new List<ItemSet>();

            foreach (var item in this.Closure) {
                var key = (item.Rule, item.Ptr);
                if (lookup.ContainsKey(key)) {
                    lookup[key].Lookahead.UnionWith(item.Lookahead);
                    remove.Add(item);
                } else {
                    lookup[key] = item;
                }
            }

            foreach (var item in remove) {
                Closure.Remove(item);
            }
        }

        public Dictionary<Symbol, Kernel> GroupRulesBySymbolAfter() {
            var groups = new Dictionary<Symbol, HashSet<ItemSet>>();

            foreach (var item in Closure) {
                var key = item.SymbolAfter;
                if (key == null) {
                    continue;
                }
                if (!groups.ContainsKey(key)) {
                    groups[key] = new HashSet<ItemSet>();
                }

                groups[key].Add(new ItemSet(item.Rule, item.Ptr + 1, item.Lookahead));
            }

            return new Dictionary<Symbol, Kernel>(groups.Select(entry => new KeyValuePair<Symbol, Kernel>(entry.Key, new Kernel(entry.Value))));
        }
    }
}
