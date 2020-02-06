using Core;
using System.Collections.Generic;
using System.Linq;

namespace Automata
{
    public abstract class AutomataTable
    {
        protected readonly HashSet<Node> _nodes;
        protected readonly List<Transition> _transitions;
        public IEnumerable<Node> Nodes => this._nodes;
        public IEnumerable<Transition> Transitions => this._transitions;
        protected int _idCounter = 0;

        private Node? _startState = null;
        public Node StartState {
            get {
                Debug.Assert(this._startState != null, "Start state cannot be null.");
                return this._startState;
            }
            set {
                Debug.Assert(this._nodes.Contains(value), "Unable to set start state as a node that does not belong to the table.");
                this._startState = value;
            }
        }

        public AutomataTable() {
            this._nodes = new HashSet<Node>();
            this._transitions = new List<Transition>();
        }

        public AutomataTable(AutomataTable table) : this() {
            this._idCounter = table._idCounter;
            var map = new Dictionary<Node, Node>();
            foreach (var node in table.Nodes) {
                var copy = new Node(node);
                map[node] = copy;
                this._nodes.Add(copy);
            }
            this.StartState = map[table.StartState];
            foreach (var transition in table.Transitions) {
                var source = map[transition.Source];
                var destination = map[transition.Destination];
                if (transition.Symbol == null) {
                    this._transitions.Add(Transition.New(source, destination));
                } else {
                    this._transitions.Add(Transition.New(source, destination, transition.Symbol));
                }
            }
        }

        public Node CreateNode() {
            var node = new Node(this._idCounter++);
            this._nodes.Add(node);
            return node;
        }

        public Node[] CreateNodes(int num) {
            Debug.Assert(num >= 0, $"Unable to create a negative amount({num}) of nodes.");
            var nodes = new Node[num];
            for (int i = 0; i < num; i++) {
                nodes[i] = this.CreateNode();
            }
            return nodes;
        }

        public virtual void AddTransition(Node a, Node b, string symbol) {
            Debug.Assert(this._nodes.Contains(a), "Unable to make a transition from node a->b when a does not belong to the table.");
            Debug.Assert(this._nodes.Contains(b), "Unable to make a transition from node a->b when b does not belong to the table.");
            this._transitions.Add(Transition.New(a, b, symbol));
        }

        public HashSet<string?> GetAllPossibleSymbols() {
            var set = new HashSet<string?>();
            foreach (var transition in this.Transitions) {
                set.Add(transition.Symbol);
            }
            return set;
        }

        public IEnumerable<Transition> GetTransitionsFor(Node node) {
            foreach (var transition in this.Transitions) {
                if (transition.Source == node) {
                    yield return transition;
                }
            }
        }

        public HashSet<Node> ApplyTransition(HashSet<Node> states, string symbol) {
            var newStates = new HashSet<Node>();

            foreach (var node in states) {
                Debug.Assert(this._nodes.Contains(node), "Unable to apply transition on target not present in a table.");
                foreach (var transition in this.GetTransitionsFor(node).Where(t => t.Symbol == symbol)) {
                    Debug.Assert(this._nodes.Contains(transition.Destination), "Unable to apply transition on destination not present in a table.");
                    newStates.Add(transition.Destination);
                }
            }

            return newStates;
        }

        public HashSet<Node> ApplyTransition(Node state, string symbol) {
            return ApplyTransition(new HashSet<Node>() { state }, symbol);
        }

        internal void RemoveNode(Node node) {
            Debug.Assert(this._nodes.Contains(node), "Cannot remove node that is not a part of the table.");
            this._nodes.Remove(node);
            this._transitions.RemoveAll(transition => transition.Destination == node || transition.Source == node);
        }
    }
}
