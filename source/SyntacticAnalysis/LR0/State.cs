using System.Collections.Generic;

namespace SyntacticAnalysis.LR0
{
    public class State
    {
        public readonly HashSet<ItemSet> Pairs;
        public readonly Dictionary<string, State> Transitions;

        public State() {
            this.Pairs = new HashSet<ItemSet>();
            this.Transitions = new Dictionary<string, State>();
        }

        public override bool Equals(object? obj) {
            if (obj is State s) {
                if (this == s) {
                    return true;
                }

                if (this.Pairs.Count != s.Pairs.Count) {
                    return false;
                }

                foreach (var rule in this.Pairs) {
                    if (!s.Pairs.Contains(rule)) {
                        return false;
                    }
                }

                return true;
            }
            return false;
        }

        public override int GetHashCode() {
            int sum = 0;
            foreach (var pair in this.Pairs) {
                sum += pair.Rule.TextRepresentation.GetHashCode();
            }
            return sum;
        }

        public void AddTransition(Symbol symbol, State nextState) {
            this.Transitions[symbol.ID] = nextState;
        }

        public State? GetTransition(Symbol symbol) {
            if (this.Transitions.ContainsKey(symbol.ID)) {
                return this.Transitions[symbol.ID];
            }
            return null;
        }
    }
}