using Core.Config;
using Core.Logging;

namespace SyntacticAnalysis
{
    public class SyntacticConfigurationFile : RuleConfigurationFile
    {
        public const string RULE_START_KEY = "start";
        public const char RULE_START_VALUE = 's';

        public const string RULE_PRODUCTION_PREFIX_KEY = "production_prefix";
        public const char RULE_PRODUCTION_PREFIX_VALUE = '$';

        public const string RULE_INLINE_KEY = "inline";
        public const char RULE_INLINE_VALUE = '^';

        public const string SECTION_TAG_PRODUCTION = "production";
        public const char TOKEN_TAG_SEPARATOR = ':';

        public const string SECTION_TAG_BLACKLIST = "blacklist";

        public const string HEADER_EPSILON_PREFIX = "epsilon:";

        public SyntacticConfigurationFile(string path, Log? log) : base(path, log) {
            this.DefineDefaultRules();
        }

        public SyntacticConfigurationFile(string[] contents, string name, Log? log) : base(contents, name, log) {
            this.DefineDefaultRules();
        }

        private void DefineDefaultRules() {
            this.SetRuleIfNotExists(RULE_START_KEY, RULE_START_VALUE);
            this.SetRuleIfNotExists(RULE_INLINE_KEY, RULE_INLINE_VALUE);
            this.SetRuleIfNotExists(RULE_PRODUCTION_PREFIX_KEY, RULE_PRODUCTION_PREFIX_VALUE);
        }
    }
}
