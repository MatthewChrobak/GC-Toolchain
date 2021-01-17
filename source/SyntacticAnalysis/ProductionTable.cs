using Core;
using Core.Config;
using Core.Logging;
using Core.ReportGeneration;
using System.Collections.Generic;
using System.Linq;

namespace SyntacticAnalysis
{
    public class ProductionTable
    {
        private Dictionary<string, Production> _productions;
        public IEnumerable<Production> Productions => this._productions.Values;

        private Log? _log;

        public const string START_PRIME = "'";

        private readonly Dictionary<string, HashSet<Symbol>> _first;

        public ProductionTable(SyntacticConfigurationFile config, Log? log) {
            this._productions = new Dictionary<string, Production>();
            this._first = new Dictionary<string, HashSet<Symbol>>();

            this.AddExtraStartProduction(config);

            foreach (var section in config.GetSections(SyntacticConfigurationFile.SECTION_TAG_PRODUCTION)) {
                ProcessSection(section, config);
            }

            this.TransformEpsilonTransitions(config);
            this.ComputeFirst();
            this._log = log;
        }

        private void TransformEpsilonTransitions(SyntacticConfigurationFile config) {
            var productionsWithEpsilonTransitions = new HashSet<string>(this._productions.Where(entry => entry.Value.CanBeEpsilon).Select(entry => entry.Key));

            foreach (var entry in this._productions) {
                var allNewRules = new List<Rule?>();
                foreach (var rule in entry.Value.Rules) {
                    foreach (var newRule in Permute(rule, 0, productionsWithEpsilonTransitions, false)) {
                        allNewRules.Add(newRule);
                    }
                }

                foreach (var newRule in allNewRules.Where(newRule => newRule != null)) {
                    if (!entry.Value.Rules.Contains(newRule)) {
                        entry.Value.Rules.Add(newRule);
                    }
                }
            }
        }

        private IEnumerable<Rule?> Permute(Rule rule, int ptr, HashSet<string> productionsWithEpsilonTransitions, bool isDifferent) {
            var symbol = rule.SymbolAfter(ptr);

            if (symbol == null) {
                if (isDifferent) {
                    yield return rule;
                } else {
                    yield return null;
                }
            } else if (symbol.Type == SymbolType.Production && productionsWithEpsilonTransitions.Contains(symbol.ID)) {
                var copy = rule.CopyWithoutSymbolAt(ptr);
                foreach (var result in Permute(copy, ptr, productionsWithEpsilonTransitions, true)) {
                    yield return result;
                }
                foreach (var result in Permute(rule, ptr + 1, productionsWithEpsilonTransitions, isDifferent)) {
                    yield return result;
                }

            } else {
                foreach (var result in Permute(rule, ptr + 1, productionsWithEpsilonTransitions, isDifferent)) {
                    yield return result;
                }
            }
        }

        public HashSet<Symbol> GetFirst(Symbol key) {
            return this._first[key.ID];
        }

        public ReportSection GetReportSection() {
            return new SyntacticGrammarReportSection(this._productions);
        }

        private void ComputeFirst() {
            var isFinished = new HashSet<string>();

            // Populate the firsts.
            foreach (var entry in this._productions) {
                var set = new HashSet<Symbol>();
                this._first[entry.Value.Key.ID] = set;

                foreach (var rule in entry.Value.Rules) {
                    if (rule.Symbols.Count != 0) {
                        set.Add(rule.Symbols.First());
                    }
                }

                set.RemoveWhere(symbol => symbol.Type == SymbolType.Production && symbol.ID == entry.Key);
            }

            while (true) {
                bool changeMade = false;
                foreach (var entry in this._first) {
                    // If all the symbols are tokens, or we're already done, we're good.
                    if (isFinished.Contains(entry.Key)) {
                        continue;
                    }
                    if (!entry.Value.Any(symbol => symbol.Type == SymbolType.Production)) {
                        changeMade = true;
                        isFinished.Add(entry.Key);
                    }

                    // Figure out which ones we can add.
                    var unknowns = entry.Value.Where(symbol => symbol.Type == SymbolType.Production).ToArray();
                    bool canBeFinished = true;
                    foreach (var symbol in unknowns) {
                        if (!isFinished.Contains(symbol.ID)) {
                            canBeFinished = false;
                            break;
                        }
                    }
                    if (!canBeFinished) {
                        continue;
                    }

                    foreach (var symbol in unknowns) {
                        entry.Value.Remove(symbol);
                        foreach (var terminal in this._first[symbol.ID]) {
                            entry.Value.Add(terminal);
                        }
                    }
                    changeMade = true;
                    isFinished.Add(entry.Key);
                }

                if (!changeMade) {
                    Debug.Assert(isFinished.Count == _first.Count, $"Unable to calculate first set: {string.Join(", ", this._first.Where(entry => !isFinished.Contains(entry.Key)).Select(entry => entry.Key))}");
                    break;
                }
            }
        }

        private void AddExtraStartProduction(SyntacticConfigurationFile config) {
            // Insert the S' -> S production to guarentee one /single/ rule in the start production.
            string start = config.GetRule(SyntacticConfigurationFile.RULE_START_KEY).ToString();
            string realStart = start + START_PRIME;
            char prefix = config.GetRule(SyntacticConfigurationFile.RULE_PRODUCTION_PREFIX_KEY);
            var section = new ConfigSection(
                $"#{SyntacticConfigurationFile.SECTION_TAG_PRODUCTION} {realStart}",
                new string[] { $"{prefix}{start}" },
                $"({nameof(ProductionTable)}.cs)",
                -1
            );
            ProcessSection(section, config);
        }

        private void ProcessSection(ConfigSection section, SyntacticConfigurationFile config) {
            Debug.Assert(section.Header.Any(entry => !string.IsNullOrEmpty(entry)), $"Cannot have empty {SyntacticConfigurationFile.SECTION_TAG_PRODUCTION} header at {section.GetLocation()}");
            string key = section.Header.First();

            var production = new Production(section, config, this._log);
            this._productions.Add(key, production);
        }

        public Production? GetProduction(string key) {
            if (!this._productions.ContainsKey(key)) {
                return null;
            }
            return this._productions[key];
        }
    }
}
