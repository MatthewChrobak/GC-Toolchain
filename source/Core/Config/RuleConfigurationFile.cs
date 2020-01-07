using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core.Config
{
    // Rule configuration files have 'rule' sections whose body define 'constants' for a particular rule.
    public class RuleConfigurationFile : ConfigurationFile
    {
        public const string SECTION_TAG_RULE = "rule";
        private readonly Dictionary<string, char> _rules = new Dictionary<string, char>();

        public RuleConfigurationFile(string configurationFilePath) : this(File.ReadAllLines(configurationFilePath), new FileInfo(configurationFilePath).Name) {
        }

        public RuleConfigurationFile(string[] configurationFileContents, string configurationFileName) : base(configurationFileContents, configurationFileName) {
            foreach (var section in this.GetSections(SECTION_TAG_RULE)) {
                string ruleKey = section.Header.FirstOrDefault();
                if (String.IsNullOrEmpty(ruleKey)) {
                    Log.WriteLineWarning($"Unable to find rule key on line {section.LineNumber}.");
                    continue;
                }

                string ruleValueString = section.Body.FirstOrDefault();
                if (String.IsNullOrEmpty(ruleValueString)) {
                    Log.WriteLineWarning($"Unable to find rule value for rule {ruleKey} on line {section.LineNumber}.");
                    continue;
                }
                if (ruleValueString.Length != 1) {
                    Log.WriteLineWarning($"Rule values can only be one character long. Shortening the rule {ruleKey} from '{ruleValueString}' to '{ruleValueString[0]}'.");
                }
                char ruleValue = ruleValueString[0];

                if (this.RuleExists(ruleKey)) {
                    this.OverrideRule(ruleKey, ruleValue);
                } else {
                    this.SetRule(ruleKey, ruleValue);
                }
            }
        }

        public bool RuleExists(string ruleKey) {
            return this._rules.ContainsKey(ruleKey);
        }

        public char GetRule(string ruleKey) {
            Debug.Assert(this.RuleExists(ruleKey), $"Unable to find rule definition for {ruleKey}");
            return this._rules[ruleKey];
        }

        private void SetRule(string ruleKey, char ruleValue) {
            Debug.Assert(!this.RuleExists(ruleKey), $"Unable to set the rule {ruleKey} which already exists");
            Log.WriteLineVerbose($"Setting {ruleKey} rule as {ruleValue}");
            this._rules[ruleKey] = ruleValue;
        }

        private void OverrideRule(string ruleKey, char ruleValue) {
            Debug.Assert(this.RuleExists(ruleKey), $"Unable to override the rule {ruleKey} which does not exist");
            Log.WriteLineWarning($"Changing {ruleKey} rule from {this.GetRule(ruleKey)} to {ruleValue}");
            this._rules[ruleKey] = ruleValue;
        }

        protected void SetRuleIfNotExists(string ruleKey, char ruleValue) {
            if (this.RuleExists(ruleKey)) {
                return;
            }
            this.SetRule(ruleKey, ruleValue);
        }
    }
}
