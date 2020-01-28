using Core.ReportGeneration;
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

        public ReportSection GetReportSection() {
            return new CLRStatesReportSection(this.States);
        }
    }
}
