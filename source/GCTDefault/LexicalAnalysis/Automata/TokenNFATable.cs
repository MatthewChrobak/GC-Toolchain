using Automata;
using Automata.NonDeterministic;
using Core;
using System.Linq;

namespace GCTDefault.LexicalAnalysis.Automata
{
    public class TokenNFATable : NFATable
    {
        public Node EndState {
            get {
                Debug.Assert(this._nodes.Count != 0, "Unable to get the last node of an empty table.");
                return this._nodes.Last();
            }
        }

        public TokenNFATable() : base() {
        }

        public TokenNFATable(AutomataTable table) : base(table) {

        }


        public void InsertTableTransition(Node ptr, TokenNFATable nfa) {
            int shiftNumber = this._idCounter;
            nfa = new TokenNFATable(nfa);

            // Make all nodes members of the current NFA.
            foreach (var node in nfa.Nodes) {
                node.ID += shiftNumber;
                this._nodes.Add(node);
            }

            // Add the transitions. Since we're working on a copy,
            // we retain the reference integrity and relations.
            foreach (var transition in nfa.Transitions) {
                this._transitions.Add(transition);
            }

            // Update the id counter. Ugly, but necessary.
            this._idCounter = EndState.ID + 1;

            // Connect to the table.
            this.AddTransition(ptr, nfa.StartState);
        }

        public Node InsertTableTransition_AndCreateAnEndState(Node ptr, TokenNFATable nfa) {
            this.InsertTableTransition(ptr, nfa);

            ptr = EndState;
            var end = this.CreateNode();
            this.AddTransition(ptr, end);

            return end;
        }
    }
}
