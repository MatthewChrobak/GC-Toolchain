using Core;
using Core.Config;
using Core.Logging;
using System.Collections.Generic;
using System.Linq;

namespace SyntacticAnalysis
{
    public class Production
    {
        public readonly Symbol Key;
        public readonly List<Rule> Rules;
        public readonly bool CanBeEpsilon;

        public Production(ConfigSection section, SyntacticConfigurationFile config, Log? log) {
            this.Key = new Symbol(config.GetRule(SyntacticConfigurationFile.RULE_PRODUCTION_PREFIX_KEY) + section.Header.First(), config);
            this.Rules = new List<Rule>();

            foreach (var entry in section.Header) {
                if (entry.StartsWith(SyntacticConfigurationFile.HEADER_EPSILON_PREFIX)) {
                    string value = entry.Remove(0, SyntacticConfigurationFile.HEADER_EPSILON_PREFIX.Length);
                    if (bool.TryParse(value, out bool canBeEpsilon)) {
                        this.CanBeEpsilon = canBeEpsilon;
                    } else {
                        log?.WriteLineWarning($"Unable to determine if {this.Key.ID} production can be epsilon or not from: '{value}'.");
                    }
                }
            }

            foreach (var line in section.Body.Where(line => !string.IsNullOrWhiteSpace(line))) {
                var rule = new Rule(this.Key, line, config);
                this.Rules.Add(rule);
            }
        }
    }
}