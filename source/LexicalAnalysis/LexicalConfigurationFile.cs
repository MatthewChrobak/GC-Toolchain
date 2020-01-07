using Core.Config;

namespace LexicalAnalysis
{
    public class LexicalConfigurationFile : RuleConfigurationFile
    {
        public const string SECTION_TAG_TOKEN = "token";
        public const string SECTION_TAG_SUBTOKEN = "subtoken";

        public LexicalConfigurationFile(string configurationFilePath) : base(configurationFilePath) {
        }

        public LexicalConfigurationFile(string[] configurationFileContents, string configurationFileName) : base(configurationFileContents, configurationFileName) {
        }
    }
}