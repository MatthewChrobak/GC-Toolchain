using Core;
using System.Collections.Generic;
using System.Linq;

namespace SyntacticAnalysis.CLR
{
    public class CLRStateGenerator
    {
        private readonly ProductionTable _productionTable;
        public readonly Dictionary<Kernel, State> States;

        public CLRStateGenerator(ProductionTable productionTable, SyntacticConfigurationFile config) {
            this._productionTable = productionTable;
            this.States = new Dictionary<Kernel, State>();

            var startSymbol = config.GetRule(SyntacticConfigurationFile.RULE_START_KEY) + ProductionTable.START_PRIME;
            var startProduction = productionTable.GetProduction(startSymbol);
            var startRule = startProduction.Rules.First();

            CreateState("", new Kernel(new ItemSet(startRule, 0)));

            //Log.WriteLineVerboseClean("");
            //Log.WriteLineVerboseClean($"Num states: {States.Count}");
            //foreach (var state in this.States) {
            //    Log.WriteLineVerboseClean("");
            //    Log.WriteLineVerboseClean($"[{state.Value.ID}]");
            //    foreach (var itemset in state.Value.Closure) {
            //        Log.WriteLineVerboseClean($"{itemset.Rule.Key.ID} -> {itemset.Rule.ToStringWithSymbol(itemset.Ptr)}, {string.Join(',', itemset.Lookahead.Select(l => l.ID))}");
            //    }
            //}
        }

        private void CreateState(string id, Kernel kernel) {
            // Make sure the state doesn't already exist.
            if (this.States.ContainsKey(kernel)) {
                return;
            }

            var state = new State(id, kernel);
            state.ComputeClosure(this._productionTable);
            state.MergeLookaheads();
            this.States.Add(kernel, state);

            foreach (var group in state.GroupRulesBySymbolAfter()) {
                CreateState(id + group.Key.ID, group.Value);
            }
        }
    }
}
