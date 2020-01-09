using System.Collections.Generic;
using System.Linq;

namespace Automata.NonDeterministic
{
    public class DFATable : AutomataTable
    {
        public DFATable Minimize() {
            var minimizer = new DFAMinimizationTable(this);
            minimizer.Minimize();
            return minimizer.ToDFATable();
        }

        public NFATable RemoveTrapStates() {
            var nfa = new NFATable(this);

            var removeNodes = new List<Node>();
            foreach (var node in nfa.Nodes.Where(n => !n.IsFinal)) {
                bool? isTrap = null;

                foreach (var transition in nfa.GetTransitionsFor(node)) {
                    if (isTrap == null) {
                        isTrap = true;
                    }
                    if (transition.Destination != node) {
                        isTrap = false;
                        break;
                    }
                }

                if (isTrap == true) {
                    removeNodes.Add(node);
                }
            }
            foreach (var node in removeNodes) {
                nfa.RemoveNode(node);
            }

            return nfa;
        }
    }
}
