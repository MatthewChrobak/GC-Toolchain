using Core;
using Core.Config;
using Core.ReportGeneration;
using System.Collections.Generic;
using System.Linq;

namespace SyntacticAnalysis
{
    public class ProductionTable
    {
        private Dictionary<string, Production> _productions;
        public IEnumerable<Production> Productions => this._productions.Values;
        public const string START_PRIME = "'";

        private readonly Dictionary<string, HashSet<Symbol>> _first;

        public ProductionTable(SyntacticConfigurationFile config) {
            this._productions = new Dictionary<string, Production>();
            this._first = new Dictionary<string, HashSet<Symbol>>();

            this.AddExtraStartProduction(config);

            foreach (var section in config.GetSections(SyntacticConfigurationFile.SECTION_TAG_PRODUCTION)) {
                ProcessSection(section, config);
            }

            this.TransformEpsilonTransitions(config);
        }

        private void TransformEpsilonTransitions(SyntacticConfigurationFile config) {
            var productionsWithEpsilonTransitions = new HashSet<string>(this._productions.Where(entry => entry.Value.CanBeEpsilon).Select(entry => entry.Key));

            foreach (var entry in this._productions) {
                IEnumerable<Rule>? newRules = null;
                foreach (var rule in entry.Value.Rules) {
                    newRules = Permute(rule, 0, productionsWithEpsilonTransitions, false);
                }

                if (newRules == null) {
                    continue;
                }

                foreach (var newRule in newRules.Where(newRule => newRule != null)) {
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
                foreach (var result in Permute(copy, ptr + 1, productionsWithEpsilonTransitions, true)) {
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

        private HashSet<Symbol>? ComputeFollow(Symbol symbol) {
            var set = new HashSet<Symbol>();

            foreach (var production in this._productions.Values) {
                foreach (var rule in production.Rules) {
                    for (int ii = 0; ii < rule.Symbols.Count; ii++) {
                        if (rule.Symbols[ii].Equals(symbol)) {

                            var after = rule.SymbolAfter(ii + 1);

                            // FOLLOW(A)
                            // if B => cAb where b is epsilon then add FOLLOW(A) = FOLLOW(B)
                            if (after == null) {
                                var key = rule.Key;

                                // is FOLLOW(B) defined?
                                if (this._follow.ContainsKey(key)) {
                                    set.UnionWith(this._follow[key]);
                                } else {
                                    if (key.Equals(symbol)) {
                                        continue;
                                    }
                                    // It's not. Exit out for now.
                                    Log.WriteLineVerboseClean($"In order to calculate FOLLOW({symbol.ID}) I need FOLLOW({key.ID})");
                                    return null;
                                }
                            } else {

                                // add FOLLOW(A) = FIRST(B)
                                if (after.Type == SymbolType.Production) {
                                    foreach (var s in this._first[after.ID]) {
                                        set.Add(s);
                                    }
                                } else {
                                    set.Add(after);
                                }
                            }
                        }
                    }
                }
            }

            return set;
        }

        public ReportSection GetReportSection() {
            return new SyntacticGrammarReportSection(this._productions);
        }

        public void ComputeFirstOld() {
            var pendingFirsts = new Queue<Symbol>(this._productions.Values.Select(production => production.Key));

            // Keep going until there's no more symbols left to process
            while (pendingFirsts.Count != 0) {
                int count = pendingFirsts.Count;

                // Make a pass
                for (int i = 0; i < count; i++) {

                    // Try and construct the symbol's first
                    var symbol = pendingFirsts.Dequeue();
                    var visited = new HashSet<Symbol>() { symbol };
                    var set = new HashSet<Symbol>();

                    foreach (var rule in this.GetProduction(symbol.ID).Rules) {
                        Debug.Assert(rule.Symbols.Count != 0, $"Syntactic grammar rule {rule.Key.ID} needs to have a first that's not epsilon");
                        set.Add(rule.Symbols.First());
                    }

                    while (true) {
                        // Is everything a token?
                        if (!set.Any(s => s.Type == SymbolType.Production)) {
                            this._first[symbol.ID] = set;
                            break;
                        }

                        var firstSymbolsToProcess = set.Where(s => s.Type == SymbolType.Production).ToArray();
                        bool exit = false;
                        foreach (var s in firstSymbolsToProcess) {
                            if (visited.Contains(s)) {
                                set.Remove(s);
                                continue;
                            }
                            visited.Add(s);

                            if (!this._first.ContainsKey(s.ID)) {
                                exit = true;
                                break;
                            }
                            foreach (var first in this._first[s.ID]) {
                                set.Add(first);
                            }
                            set.Remove(s);
                        }
                        if (exit) {
                            break;
                        }
                    }

                    if (!this._first.ContainsKey(symbol.ID)) {
                        pendingFirsts.Enqueue(symbol);
                    }
                }

                if (pendingFirsts.Count == count) {
                    Debug.Assert(count != pendingFirsts.Count, $"Unable to calculate first set: {string.Join(',', pendingFirsts.Select(first => first.ID))}");
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

            var production = new Production(section, config);
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
