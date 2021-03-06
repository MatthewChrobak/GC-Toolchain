﻿using Core.Config;

namespace LexicalAnalysis
{
    public class LexicalConfigurationFile : RuleConfigurationFile
    {
        public const string SECTION_TAG_TOKEN = "token";
        public const string SECTION_TAG_SUBTOKEN = "subtoken";

        public const string RULE_LITERAL_PREFIX_KEY = "literal_prefix";
        public const char RULE_LITERAL_PREFIX_VALUE = '\\';

        public const string RULE_RANGE_INCLUSIVE_KEY = "range_inclusive";
        public const char RULE_RANGE_INCLUSIVE_VALUE = '-';

        public const string RULE_SUB_TOKEN_PREFIX_KEY = "sub_token_prefix";
        public const char RULE_TOKEN_PREFIX_VALUE = '$';

        public const string RULE_ZERO_OR_MORE_KEY = "zero_or_more";
        public const char RULE_ZERO_OR_MORE_VALUE = '*';

        public const string RULE_START_KEY = "start";
        public const char RULE_START_VALUE = 's';

        public const string RULE_HEX_PREFIX_KEY = "hex_prefix";
        public const char RULE_HEX_PREFIX_VALUE = '%';

        public const string HEADER_PRIORITY_PREFIX = "priority:";

        public LexicalConfigurationFile(string configurationFilePath) : base(configurationFilePath) {
            this.DefineDefaultRules();
        }

        public LexicalConfigurationFile(string[] configurationFileContents, string configurationFileName) : base(configurationFileContents, configurationFileName) {
            this.DefineDefaultRules();
        }

        private void DefineDefaultRules() {
            this.SetRuleIfNotExists(RULE_HEX_PREFIX_KEY, RULE_HEX_PREFIX_VALUE);
            this.SetRuleIfNotExists(RULE_LITERAL_PREFIX_KEY, RULE_LITERAL_PREFIX_VALUE);
            this.SetRuleIfNotExists(RULE_RANGE_INCLUSIVE_KEY, RULE_RANGE_INCLUSIVE_VALUE);
            this.SetRuleIfNotExists(RULE_START_KEY, RULE_START_VALUE);
            this.SetRuleIfNotExists(RULE_SUB_TOKEN_PREFIX_KEY, RULE_TOKEN_PREFIX_VALUE);
            this.SetRuleIfNotExists(RULE_ZERO_OR_MORE_KEY, RULE_ZERO_OR_MORE_VALUE);
        }
    }
}