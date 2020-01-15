using Core;
using System.Collections.Generic;
using System.Linq;

namespace Automata.NonDeterministic
{
    internal class DFAMinimizationTable
    {
        private DFATable _dfa;
        private readonly Dictionary<NodePair, Relation> _relations;

        public DFAMinimizationTable(DFATable dFATable) {
            this._dfa = dFATable;
            this._relations = new Dictionary<NodePair, Relation>();
        }

        internal DFATable ToDFATable() {
            var dfa = new DFATable();

            var mergedStates = new List<HashSet<Node>>();
            foreach (var entry in this._relations) {
                if (entry.Value != Relation.Equivalent) {
                    continue;
                }
                var pair = entry.Key;
                bool merged = false;
                foreach (var mergedState in mergedStates) {
                    if (mergedState.Contains(pair.A) || mergedState.Contains(pair.B)) {
                        mergedState.Add(pair.A);
                        mergedState.Add(pair.B);
                        merged = true;
                        break;
                    }
                }

                if (!merged) {
                    mergedStates.Add(new HashSet<Node>() { pair.A, pair.B });
                }
            }

            // Create new states.
            var mergedStateMap = new Dictionary<Node, Node>();
            foreach (var mergedState in mergedStates) {
                var node = dfa.CreateNode();

                foreach (var state in mergedState) {
                    node.UnionTags(state.GetTags());
                    mergedStateMap[state] = node;
                }

                // Merged states will not have conflicting values for IsFinal.
                node.IsFinal = mergedState.First().IsFinal;
            }

            var symbols = this._dfa.GetAllPossibleSymbols();
            foreach (var mergedState in mergedStates) {
                foreach (var symbol in symbols) {
                    Debug.Assert(symbol != NFATable.Epsilon, $"DFATable cannot contain epsilon transition.");
                    var resultingStates = this._dfa.ApplyTransition(mergedState, symbol);

                    var source = mergedStateMap[mergedState.First()];
                    var destination = mergedStateMap[resultingStates.First()];

                    dfa.AddTransition(source, destination, symbol);
                }
            }

            dfa.StartState = mergedStateMap[this._dfa.StartState];
            return dfa;
        }

        internal void Minimize() {
            var symbols = this._dfa.GetAllPossibleSymbols();

            // Pre-mark equivalence relations.
            foreach (var nodeA in this._dfa.Nodes) {
                foreach (var nodeB in this._dfa.Nodes) {
                    var entry = new NodePair(nodeA, nodeB);
                    if (nodeA == nodeB) {
                        this._relations[entry] = Relation.Equivalent;
                        continue;
                    }
                    if (entry.A.IsFinal != entry.B.IsFinal) {
                        this._relations[entry] = Relation.Distinguishable;
                    } else {
                        this._relations[entry] = Relation.Equivalent;
                    }
                }
            }

            bool changeMade;
            do {
                changeMade = false;
                foreach (var a in this._dfa.Nodes) {
                    foreach (var b in this._dfa.Nodes) {
                        if (a == b) {
                            continue;
                        }
                        var entry = new NodePair(a, b);
                        if (this._relations[entry] == Relation.Distinguishable) {
                            continue;
                        }
                        foreach (var symbol in symbols) {
                            Debug.Assert(symbol != NFATable.Epsilon, "DFATable cannot have an epsilon transition.");
                            var aDestinations = this._dfa.ApplyTransition(a, symbol);
                            var bDestinations = this._dfa.ApplyTransition(b, symbol);

                            Debug.Assert(aDestinations.Count == 1, "Found multiple aDestinations for a DFA");
                            Debug.Assert(bDestinations.Count == 1, "Found multiple bDestinations for a DFA");

                            var aDestination = aDestinations.First();
                            var bDestination = bDestinations.First();
                            var entryDestination = new NodePair(aDestination, bDestination);

                            if (this._relations[entryDestination] == Relation.Distinguishable) {
                                this._relations[entry] = Relation.Distinguishable;
                                changeMade = true;
                            }
                        }
                    }
                }
            } while (changeMade);
        }
    }

    internal class NodePair
    {
        public readonly Node A;
        public readonly Node B;

        public NodePair(Node a, Node b) {
            if (a.ID <= b.ID) {
                this.A = a;
                this.B = b;
            } else {
                this.B = a;
                this.A = b;
            }
        }

        public override bool Equals(object? obj) {
            if (obj is NodePair pair) {
                return pair.A == this.A && pair.B == this.B;
            }
            return false;
        }

        public override int GetHashCode() {
            return this.A.ID + this.B.ID;
        }

        public override string ToString() {
            return $"{{{this.A.ID},{this.B.ID}}}";
        }
    }

    public enum Relation
    {
        Equivalent,
        Distinguishable
    }
}