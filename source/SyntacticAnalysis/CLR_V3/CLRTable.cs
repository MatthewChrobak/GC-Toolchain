using Core;
using System.Collections.Generic;
using System.Linq;

namespace SyntacticAnalysis.CLR_V3
{
    public class CLRTable
    {
        private readonly ProductionTable _productionTable;
        private readonly Dictionary<Kernel, State> _states;

        public CLRTable(ProductionTable productionTable, SyntacticConfigurationFile config) {
            this._productionTable = productionTable;
            this._states = new Dictionary<Kernel, State>();

            var startSymbol = config.GetRule(SyntacticConfigurationFile.RULE_START_KEY) + ProductionTable.START_PRIME;
            var startProduction = productionTable.GetProduction(startSymbol);
            var startRule = startProduction.Rules.First();

            CreateState("", new Kernel(new ItemSet(startRule, 0)));

            foreach (var state in this._states) {
                Log.WriteLineVerboseClean("");
                Log.WriteLineVerboseClean($"[{state.Value.ID}]");
                foreach (var itemset in state.Value.Closure) {
                    Log.WriteLineVerboseClean($"{itemset.Rule.Key.ID} -> {itemset.Rule.ToStringWithSymbol(itemset.Ptr)}, {string.Join(',', itemset.Lookahead.Select(l => l.ID))}");
                }
            }
        }

        private void CreateState(string id, Kernel kernel) {
            // Make sure the state doesn't already exist.
            if (this._states.ContainsKey(kernel)) {
                return;
            }

            var state = new State(id, kernel);
            state.ComputeClosure(this._productionTable);
            state.MergeLookaheads();
            this._states.Add(kernel, state);

            foreach (var group in state.GroupRulesBySymbolAfter()) {
                CreateState(id + group.Key, new Kernel(group.Value));
            }
        }
    }

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
                if (this.Ptr != itemset.Ptr) {
                    return false;
                }

                if (!this.Rule.Equals(itemset.Rule)) {
                    return false;
                }

                return this.Lookahead.HasSameElementsAs(itemset.Lookahead);
            }
            return false;
        }

        public override int GetHashCode() {
            return this.Ptr.GetHashCode() + this.Rule.TextRepresentation.GetHashCode() + this.Lookahead.Count;
        }
    }

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

        public Dictionary<string, HashSet<ItemSet>> GroupRulesBySymbolAfter() {
            var groups = new Dictionary<string, HashSet<ItemSet>>();

            foreach (var item in Closure) {
                var key = item.SymbolAfter?.ID;
                if (key == null) {
                    continue;
                }
                if (!groups.ContainsKey(key)) {
                    groups[key] = new HashSet<ItemSet>();
                }

                groups[key].Add(new ItemSet(item.Rule, item.Ptr + 1, item.Lookahead));
            }

            return groups;
        }
    }

    public static class Extensions
    {
        public static bool HasSameElementsAs<T>(this HashSet<T> hs1, HashSet<T> hs2) {
            if (hs1.Count != hs2.Count) {
                return false;
            }
            foreach (var element in hs1) {
                if (!hs2.Contains(element)) {
                    return false;
                }
            }
            return true;
        }
    }
}
