using Core.Config;
using System.Collections.Generic;
using System.Linq;

namespace SyntacticAnalysis
{
    public class Production
    {
        public readonly Symbol Key;
        public readonly List<Rule> Rules;

        public Production(ConfigSection section, SyntacticConfigurationFile config) {
            this.Key = new Symbol(config.GetRule(SyntacticConfigurationFile.RULE_PRODUCTION_PREFIX_KEY) + section.Header.First(), config);
            this.Rules = new List<Rule>();

            foreach (var line in section.Body.Where(line => !string.IsNullOrWhiteSpace(line))) {
                var rule = new Rule(this.Key, line, config);
                this.Rules.Add(rule);
            }
        }
    }
}