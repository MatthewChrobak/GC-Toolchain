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
            // TODO:
            return new DFATable();
        }
    }
}
