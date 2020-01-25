﻿using Core.Config;

namespace SyntacticAnalysis
{
    public class SyntacticConfigurationFile : RuleConfigurationFile
    {
        public const string RULE_START_KEY = "start";
        public const char RULE_START_VALUE = 's';

        public const string RULE_EPSILON_KEY = "epsilon";
        public const char RULE_EPSILON_VALUE = 'ε';

        public const string RULE_PRODUCTION_PREFIX_KEY = "production_prefix";
        public const char RULE_PRODUCTION_PREFIX_VALUE = '$';

        public const string SECTION_TAG_PRODUCTION = "production";
        public const char TOKEN_TAG_SEPARATOR = ':';

        public SyntacticConfigurationFile(string path) : base(path) {
            this.DefineDefaultRules();
        }

        public SyntacticConfigurationFile(string[] contents, string name) : base(contents, name) {
            this.DefineDefaultRules();
        }

        private void DefineDefaultRules() {
            this.SetRuleIfNotExists(RULE_START_KEY, RULE_START_VALUE);
            this.SetRuleIfNotExists(RULE_EPSILON_KEY, RULE_EPSILON_VALUE);
            this.SetRuleIfNotExists(RULE_PRODUCTION_PREFIX_KEY, RULE_PRODUCTION_PREFIX_VALUE);
        }
    }
}