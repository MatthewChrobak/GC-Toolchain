using System.Collections.Generic;

namespace Automata.NonDeterministic
{
    public class NFATable : AutomataTable
    {
        public const string? Epsilon = null;

        public NFATable() : base() {

        }

        public NFATable(AutomataTable table) : base(table) {

        }

        public void AddTransition(Node a, Node b) {
            this._transitions.Add(Transition.New(a, b));
        }

        public DFATable ToDFATable() {
            var eclosureTable = new EClosureTable(this);
            eclosureTable.Fill();
            return eclosureTable.ToDFATable();
        }

        public HashSet<Node> ComputeNodeEClosure(Node target) {
            var eclosure = new HashSet<Node>();
            eclosure.Add(target);

            while (true) {
                var newEclosure = new HashSet<Node>(eclosure);

                foreach (var node in eclosure) {
                    foreach (var transition in this.GetTransitionsFor(node)) {
                        if (transition.Symbol == NFATable.Epsilon) {
                            newEclosure.Add(transition.Destination);
                        }
                    }
                }

                if (newEclosure.Count == eclosure.Count) {
                    return eclosure;
                }
                eclosure = newEclosure;
            }
        }

        public HashSet<Node> ComputeNodeEClosure(HashSet<Node> targets) {
            var eclosure = new HashSet<Node>();
            foreach (var target in targets) {
                eclosure.UnionWith(this.ComputeNodeEClosure(target));
            }
            return eclosure;
        }
    }
}
