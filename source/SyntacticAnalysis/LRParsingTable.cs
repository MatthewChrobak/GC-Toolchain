using Core;
using Core.ReportGeneration;
using SyntacticAnalysis.CLR;
using System.Collections.Generic;
using System.Linq;

namespace SyntacticAnalysis
{
    public class LRParsingTable
    {
        public readonly LRParsingTableRow[] Rows;
        public readonly Rule[] Productions;

        private LRParsingTable(int stateCount, ProductionTable productionTable) {
            this.Rows = new LRParsingTableRow[stateCount];

            int numRules = productionTable.Productions.Sum(production => production.Rules.Count);
            this.Productions = new Rule[numRules];

            for (int i = 0; i < this.Rows.Length; i++) {
                this.Rows[i] = new LRParsingTableRow();
            }
        }

        public static LRParsingTable From(CLRStateGenerator stateMachine, ProductionTable productionTable) {
            var table = new LRParsingTable(stateMachine.States.Count, productionTable);
            var kernelLookup = new Dictionary<Kernel, int>();
            var ruleLookup = new Dictionary<Rule, int>();

            // Create rule lookup table
            int ptr = 0;
            foreach (var production in productionTable.Productions) {
                foreach (var rule in production.Rules) {
                    table.Productions[ptr] = rule;
                    ruleLookup[rule] = ptr++;
                }
            }

            // Create state lookup table
            ptr = 0;
            foreach (var entry in stateMachine.States) {
                kernelLookup[entry.Key] = ptr++;
            }

            foreach (var stateEntry in stateMachine.States) {
                var tableRow = table.Rows[kernelLookup[stateEntry.Key]];
                foreach (var groupEntry in stateEntry.Value.GroupRulesBySymbolAfter()) {
                    var symbol = groupEntry.Key;

                    // SHIFT?
                    if (symbol.Type == SymbolType.Token) {
                        var action = new LRParsingTableAction(ActionType.Shift, kernelLookup[groupEntry.Value]);
                        Debug.Assert(!tableRow.Actions.ContainsKey(symbol.ID) || tableRow.Actions[symbol.ID].Type != ActionType.Reduce, $"Shift/reduce conflict in the syntax grammar");
                        tableRow.Actions.Add(symbol.ID, action);
                    }
                    // GOTO?
                    if (symbol.Type == SymbolType.Production) {
                        int stateID = kernelLookup[groupEntry.Value];
                        tableRow.GOTO.Add(symbol.ID, stateID);
                    }
                }
                // REDUCE?
                foreach (var itemset in stateEntry.Value.Closure.Where(s => s.SymbolAfter == null)) {
                    foreach (var lookahead in itemset.Lookahead) {
                        Debug.Assert(!tableRow.Actions.ContainsKey(lookahead.ID) || tableRow.Actions[lookahead.ID].Type != ActionType.Shift, $"Shift/reduce conflict in the syntax grammar");
                        tableRow.Actions.Add(lookahead.ID, new LRParsingTableAction(ActionType.Reduce, ruleLookup[itemset.Rule]));
                    }
                }
            }

            table.Rows[1].Actions[Symbol.EndStream.ID] = new LRParsingTableAction(ActionType.Accept, -1);

            return table;
        }

        public ReportSection GetReportSection() {
            return new LRParsingTableReportSection(this);
        }
    }

    public class LRParsingTableRow
    {
        public Dictionary<string, LRParsingTableAction> Actions;
        public Dictionary<string, int> GOTO;

        public LRParsingTableRow() {
            this.Actions = new Dictionary<string, LRParsingTableAction>();
            this.GOTO = new Dictionary<string, int>();
        }

        public string GetAction(string terminal) {
            if (!this.Actions.ContainsKey(terminal)) {
                return "";
            }
            return this.Actions[terminal].ToString();
        }

        public string GetGOTO(string production) {
            if (!this.GOTO.ContainsKey(production)) {
                return "";
            }
            return this.GOTO[production].ToString();
        }
    }

    public class LRParsingTableAction
    {
        public readonly int ID;
        public readonly ActionType Type;

        public LRParsingTableAction(ActionType type, int id) {
            Debug.Assert(type != ActionType.Accept || id == -1, $"Accept {nameof(LRParsingTableAction)} actions must have an id of -1");
            this.Type = type;
            this.ID = id;
        }

        public override string ToString() {
            if (this.Type == ActionType.Accept) {
                return this.Type.ToString();
            }
            return this.Type.ToString()[0].ToString() + this.ID;
        }
    }

    public enum ActionType
    {
        Reduce,
        Shift,
        Accept
    }
}
