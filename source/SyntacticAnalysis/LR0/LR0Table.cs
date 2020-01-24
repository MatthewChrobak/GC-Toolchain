using Core;
using System.Collections.Generic;
using System.Linq;

namespace SyntacticAnalysis.LR0
{
    public class LR0Table
    {
        private readonly ProductionTable _productions;
        private readonly HashSet<State> _states;
        private readonly Dictionary<string, State> _newStates;

        public LR0Table(ProductionTable productionTable, SyntacticConfigurationFile config) {
            this._productions = productionTable;
            this._states = new HashSet<State>();
            this._newStates = new Dictionary<string, State>();

            string startSymbol = config.GetRule(SyntacticConfigurationFile.RULE_START_KEY) + ProductionTable.START_PRIME;
            var startState = productionTable.GetProduction(startSymbol);
            var startRule = startState.Rules.First();

            this.CreateState(startRule, 0);

            foreach (var state in this._states) {
                Log.WriteLineVerboseClean("");
                foreach (var pair in state.Pairs) {
                    Log.WriteLineVerboseClean($"{pair.Rule.Key.ID} -> {pair.Rule.ToStringWithSymbol(pair.Ptr)}");
                }
            }
        }

        private State? CreateState(Rule rule, int ptr) {
            if (ptr > rule.Symbols.Count) {
                return null;
            }

            var state = new State();
            foreach (var closurePair in this.GetClosure(rule, ptr)) {
                state.Pairs.Add(closurePair);
            }

            foreach (var existingState in this._states) {
                if (existingState.Equals(state)) {
                    return existingState;
                }
            }
            this._states.Add(state);

            ProcessPairs(state);
            return state;
        }

        private IEnumerable<ItemSet> GetClosure(Rule startRule, int ptr) {
            yield return new ItemSet(startRule, ptr);

            var symbol = startRule.SymbolAfter(ptr);
            if (symbol != null) {
                var visited = new HashSet<Symbol>();
                var todo = new Queue<Symbol>();
                todo.Enqueue(symbol);

                while (todo.Count != 0) {
                    symbol = todo.Dequeue();
                    if (visited.Contains(symbol) || symbol.Type != SymbolType.Production) {
                        continue;
                    }
                    visited.Add(symbol);

                    var production = this._productions.GetProduction(symbol.ID);
                    foreach (var rule in production.Rules) {
                        symbol = rule.Symbols[0];
                        todo.Enqueue(symbol);
                        yield return new ItemSet(rule, 0);
                    }
                }
            }
        }

        private void ProcessPairs(State state) {
            var outgoingTransisions = new Dictionary<Symbol, State>();

            foreach (var pair in state.Pairs) {
                var symbol = pair.Rule.SymbolAfter(pair.Ptr);
                if (symbol != null) {
                    State? nextState = null;
                    if (outgoingTransisions.ContainsKey(symbol)) {
                        nextState = outgoingTransisions[symbol];
                    }
                    if (state.GetTransition(symbol) != null) {
                        throw new System.Exception();
                    }
                    if (nextState == null) {
                        nextState = state.GetTransition(symbol) ?? new State();
                    }
                    if (!outgoingTransisions.ContainsKey(symbol)) {
                        outgoingTransisions[symbol] = nextState;
                    }
                    foreach (var closurePair in this.GetClosure(pair.Rule, pair.Ptr + 1)) {
                        nextState.Pairs.Add(closurePair);
                    }
                }
            }

            foreach (var entry in outgoingTransisions) {
                var symbol = entry.Key;
                var outgoingState = entry.Value;

                if (this._states.TryGetValue(outgoingState, out var existingOutgoingState)) {
                    state.AddTransition(symbol, existingOutgoingState);
                    outgoingTransisions.Remove(symbol);
                    continue;
                }

                this._states.Add(outgoingState);
                state.AddTransition(symbol, outgoingState);
            }

            foreach (var entry in outgoingTransisions) {
                ProcessPairs(entry.Value);
            }
        }
    }
}
