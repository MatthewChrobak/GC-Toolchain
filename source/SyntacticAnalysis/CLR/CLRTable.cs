using Core;
using System.Collections.Generic;
using System.Linq;

namespace SyntacticAnalysis.CLR
{
    public class CLRTable
    {
        private readonly Dictionary<ItemSet, State> _states;
        private readonly ProductionTable _productionTable;

        public CLRTable(ProductionTable productionTable, SyntacticConfigurationFile config) {
            this._productionTable = productionTable;
            this._states = new Dictionary<ItemSet, State>();

            string startRule = config.GetRule(SyntacticConfigurationFile.RULE_START_KEY) + ProductionTable.START_PRIME;
            var startState = productionTable.GetProduction(startRule);
            Debug.Assert(startState != null, $"Start symbol {startRule} not present in the grammar");
            Debug.Assert(startState.Rules.Count == 1, $"Start symbol {startRule} has {startState.Rules.Count} rules, but should have 1");

            this.CreateState(new ItemSet(startState.Rules.First(), 0/*, productionTable.GetFollow(startState.Key)*/));

            foreach (var state in this._states) {
                Log.WriteLineVerboseClean("");
                Log.WriteLineVerboseClean(state.Value.ToString());
            }
        }

        private void CreateState(ItemSet baseItemSet) {
            if (this._states.ContainsKey(baseItemSet)) {
                return;
            }
            // TODO: I seem to remember a case where we actually append a rule to a finalized state.
            // I think it was in the original COMP 442 slides. This may not be the case for CLR though. I think it was for LR(0) table construction.

            // This code is for processing and getting the closure of a single state.
            var newState = new State(baseItemSet);

            // We want to go through the base item set, and get the closure of the /non/-terminals that we see.
            // In order to do this, we need to keep track of all the non-terminals that we see to avoid looping forever. 
            var locallyVisitedSymbols = new HashSet<Symbol>();

            // Once we're done processing this state, we need to branch off into furthur states by moving the ptr of the item sets. 
            var toProcess = new HashSet<ItemSet>();

            // Keep track of the closure.
            var closureMembers = new Stack<ItemSet>();
            closureMembers.Push(baseItemSet);

            while (closureMembers.Count != 0) {
                var itemset = closureMembers.Pop();
                var symbol = itemset.NextSymbol();
                if (symbol == null) {
                    continue;
                }

                // Make sure we don't process a symbol twice.
                if (locallyVisitedSymbols.Contains(symbol)) {
                    continue;
                }
                locallyVisitedSymbols.Add(symbol);

                // There's no closure for a terminal. Make sure if the symbol is not a production, we go to the next symbol.
                var production = this._productionTable.GetProduction(symbol.ID);
                if (production == null) {
                    Debug.Assert(symbol.Type == SymbolType.Token, $"Unable to find production for '{symbol.ID}' in production table");
                    continue;
                }

                foreach (var rule in production.Rules) {
                    var nextItemSet = new ItemSet(rule, 0/*, this._productionTable.GetFollow(rule.Key)*/);
                    newState.ItemSets.Add(nextItemSet);
                    closureMembers.Push(nextItemSet);
                }
            }
            this._states.Add(baseItemSet, newState);

            foreach (var itemset in newState.ItemSets) {
                if (itemset.Ptr == itemset.Rule.Symbols.Count) {
                    continue;
                }
                this.CreateState(new ItemSet(itemset.Rule, itemset.Ptr + 1/*, this._productionTable.GetFollow(itemset.Rule.Key)*/));
            }

        }

        private IEnumerable<ItemSet> GetClosure(State state) {
            var visited = new HashSet<Symbol>();
            var stk = new Stack<ItemSet>();
            stk.Push(state.ItemSetID);

            while (stk.Count != 0) {
                var itemset = stk.Pop();
                var symbol = itemset.NextSymbol();
                if (symbol == null) {
                    continue;
                }

                if (visited.Contains(symbol)) {
                    continue;
                }
                visited.Add(symbol);

                var production = this._productionTable.GetProduction(symbol.ID);
                if (production == null) {
                    continue;
                }
                foreach (var rule in production.Rules) {
                    var nextItemsSet = new ItemSet(rule, 0/*, this._productionTable.GetFollow(rule.Key)*/);
                    yield return nextItemsSet;
                    stk.Push(nextItemsSet);
                }
            }
        }
    }
}
