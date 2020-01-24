using Core;
using System.Collections.Generic;
using System.Linq;

namespace SyntacticAnalysis.CLR_V2
{
    public class CLRTable
    {
        private ProductionTable _productionTable;
        private readonly Dictionary<string, State> _states;

        public CLRTable(ProductionTable productionTable, SyntacticConfigurationFile config) {
            this._productionTable = productionTable;
            this._states = new Dictionary<string, State>();

            var startSymbol = config.GetRule(SyntacticConfigurationFile.RULE_START_KEY).ToString();
            var startProduction = this._productionTable.GetProduction(startSymbol);
            var startRule = startProduction.Rules.First();
            var startStateIdentifier = new ItemSet(startRule, 0);

            this._states.Add("", new State(startStateIdentifier));

            while (true) {
                int pre_count = this._states.Count;

                var lateAddQueue = new Queue<(State, ItemSet)>();
                foreach (var entry in this._states) {
                    entry.Value.Populate(this._productionTable);

                    foreach (var itemset in entry.Value.ItemSets) {
                        if (itemset.Ptr < itemset.Rule.Symbols.Count) {

                            // Create the next state
                            var identifier = new ItemSet(itemset.Rule, itemset.Ptr + 1);
                            string key = identifier.Rule.ToString(identifier.Ptr);

                            if (this._states.ContainsKey(key)) {
                                lateAddQueue.Enqueue((this._states[key], identifier));
                                continue;
                            }

                            var newState = new State(identifier);
                            lateAddQueue.Enqueue((newState, identifier));
                        }
                    }
                }

                while (lateAddQueue.Count != 0) {
                    (var state, var itemset) = lateAddQueue.Dequeue();
                    state.ItemSets.Add(itemset);

                    string key = state.Identifier.Rule.ToString(state.Identifier.Ptr);
                    if (!this._states.ContainsKey(key)) {
                        this._states.Add(key, state);
                    }
                }

                if (this._states.Count == pre_count) {
                    break;
                }
            }

            foreach (var entry in this._states) {
                Log.WriteLineVerboseClean($"V[{entry.Key}]");
                foreach (var itemset in entry.Value.ItemSets) {
                    Log.WriteLineVerboseClean(itemset.ToString_WithKey());
                }
            }
        }
    }

    public class State
    {
        public readonly ItemSet Identifier;
        public readonly HashSet<ItemSet> ItemSets;

        public State(ItemSet identifier) {
            this.Identifier = identifier;
            this.ItemSets = new HashSet<ItemSet>();
        }

        public void Populate(ProductionTable productionTable) {
            var stk = new Stack<ItemSet>();
            stk.Push(this.Identifier);

            while (stk.Count != 0) {
                var itm = stk.Pop();

                if (this.ItemSets.Contains(itm)) {
                    continue;
                }
                this.ItemSets.Add(itm);

                var nextSymbol = itm.Rule.SymbolAfter(itm.Ptr);
                if (nextSymbol == null) {
                    continue;
                }

                if (nextSymbol.Type == SymbolType.Production) {
                    var production = productionTable.GetProduction(nextSymbol.ID);
                    Debug.Assert(production != null, "Null production");

                    foreach (var rule in production.Rules) {
                        stk.Push(new ItemSet(rule, 0));
                    }
                }
            }
        }
    }

    public class ItemSet
    {
        public readonly Rule Rule;
        public readonly int Ptr;

        public ItemSet(Rule rule, int ptr) {
            this.Rule = rule;
            this.Ptr = ptr;
        }

        public string ToString_WithKey() {
            return $"{Rule.Key.ID} -> {this.Rule.ToStringWithSymbol(this.Ptr)}";
        }

        public override bool Equals(object? obj) {
            if (obj is ItemSet itemset) {
                return itemset.Ptr == this.Ptr && itemset.Rule.Equals(this.Rule);
            }
            return false;
        }

        public override int GetHashCode() {
            return this.Rule.GetHashCode() + this.Ptr.GetHashCode();
        }
    }
}
