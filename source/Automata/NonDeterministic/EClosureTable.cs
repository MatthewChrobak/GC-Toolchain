using Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Automata.NonDeterministic
{
    internal class EClosureTable
    {
        private readonly NFATable _nfa;
        private readonly List<EClosureTableRow> _rows;
        private readonly HashSet<string?> _symbols;
        private readonly Dictionary<string, int> _rowMapping;

        internal EClosureTable(NFATable nfa) {
            this._nfa = nfa;
            this._rows = new List<EClosureTableRow>();
            this._symbols = nfa.GetAllPossibleSymbols();
            this._rowMapping = new Dictionary<string, int>();
        }

        internal DFATable ToDFATable() {
            var dfa = new DFATable();
            var nodes = dfa.CreateNodes(this._rows.Count);

            foreach (var row in this._rows) {
                var node = nodes[this._rowMapping[row.GetStatesIdentifier()]];
                node.IsFinal = row.IsRowFinal();

                foreach (var n in row.States) {
                    node.UnionTags(n.GetTags());
                }

                foreach (var symbol in this._symbols) {
                    if (symbol == NFATable.Epsilon) {
                        continue;
                    }

                    var source = node;
                    var destination = nodes[this._rowMapping[row.GetTransition(symbol).GetIdentifier()]];
                    dfa.AddTransition(source, destination, symbol);
                }
            }

            // First row should be the start state.
            dfa.StartState = nodes[this._rowMapping[this._rows[0].GetStatesIdentifier()]];

            return dfa;
        }

        public void Fill() {
            Debug.Assert(this._rows.Count == 0 && this._rowMapping.Count == 0, $"{nameof(EClosureTable)}:{nameof(Fill)} operation has already been done.");

            this.AddRow(this._nfa.ComputeNodeEClosure(this._nfa.StartState));

            for (int i = 0; i < this._rows.Count; i++) {
                var row = this._rows[i];
                foreach (var symbol in this._symbols) {
                    if (symbol == NFATable.Epsilon) {
                        continue;
                    }
                    row.FillTransition(symbol, this._nfa);
                    this.AddRow(row.GetTransition(symbol));
                }
            }
        }

        private void AddRow(HashSet<Node> states) {
            // If the state already exists, don't add it.
            string id = states.GetIdentifier();
            if (this._rowMapping.ContainsKey(id)) {
                return;
            }
            this._rowMapping[id] = this._rows.Count;
            this._rows.Add(new EClosureTableRow(states));
        }
    }

    internal class EClosureTableRow
    {
        private readonly HashSet<Node> _states;
        private readonly Dictionary<string, HashSet<Node>> _transitions;

        // Dictionary keys cannot be null. So use a string instead.
        private const string EPSILON_STR = "$___EPSILON___";

        public IEnumerable<Node> States => this._states;

        public EClosureTableRow(HashSet<Node> states) {
            this._states = states;
            this._transitions = new Dictionary<string, HashSet<Node>>();
        }

        public void FillTransition(string? symbol, NFATable nfa) {
            Debug.Assert(symbol != EPSILON_STR, $"Transition symbol is equal to the EClosure {nameof(EPSILON_STR)} const.");
            if (symbol == NFATable.Epsilon) {
                // TODO: What happens when we apply this transition?
                symbol = EPSILON_STR;
            }

            this._transitions[symbol] = nfa.ComputeNodeEClosure(nfa.ApplyTransition(this._states, symbol));
        }

        internal HashSet<Node> GetTransition(string? symbol) {
            if (symbol == NFATable.Epsilon) {
                symbol = EPSILON_STR;
            }
            if (!this._transitions.ContainsKey(symbol)) {
                return new HashSet<Node>();
            }
            return this._transitions[symbol];
        }

        internal string GetStatesIdentifier() {
            return this._states.GetIdentifier();
        }

        internal bool IsRowFinal() {
            return this._states.IsFinal();
        }
    }

    internal static class Extensions
    {
        internal static string GetIdentifier(this HashSet<Node> states) {
            return String.Join(",", states.OrderBy(node => node.ID).Select(node => node.ID));
        }

        internal static bool IsFinal(this HashSet<Node> states) {
            return states.Any(state => state.IsFinal);
        }
    }
}