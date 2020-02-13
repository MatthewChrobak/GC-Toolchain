using Core.ReportGeneration;
using System.Collections.Generic;
using System.Linq;

namespace SyntacticAnalysis.CLR
{
    public class CLRStateGenerator
    {
        private readonly ProductionTable _productionTable;
        public readonly Dictionary<Kernel, State> States;

        private Queue<(string id, Kernel kernel)> _queue;

        public CLRStateGenerator(ProductionTable productionTable, SyntacticConfigurationFile config) {
            this._productionTable = productionTable;
            this.States = new Dictionary<Kernel, State>();

            var startSymbol = config.GetRule(SyntacticConfigurationFile.RULE_START_KEY) + ProductionTable.START_PRIME;
            var startProduction = productionTable.GetProduction(startSymbol);
            var startRule = startProduction.Rules.First();

            this._queue = new Queue<(string id, Kernel kernel)>();
            this._queue.Enqueue(("", new Kernel(new ItemSet(startRule, 0))));

            while (this._queue.Count != 0) {
                (string id, var kernel) = this._queue.Dequeue();
                CreateState(id, kernel);
            }
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
                this._queue.Enqueue((id + group.Key.ID, group.Value));
            }
        }

        public ReportSection GetReportSection() {
            return new CLRStatesReportSection(this.States);
        }

        public string ToTestString() {
            return new CLRStatesReportSection(this.States).ToTestString();
        }
    }
}
