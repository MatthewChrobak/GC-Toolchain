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
        private readonly Dictionary<string, string> _rules = new Dictionary<string, string>();

        public RuleConfigurationFile(string configurationFilePath) : base(File.ReadAllLines(configurationFilePath), new FileInfo(configurationFilePath).Name) {
        }

        public RuleConfigurationFile(string[] configurationFileContents, string configurationFileName) : base(configurationFileContents, configurationFileName) {
            foreach (var section in this.GetSections(SECTION_TAG_RULE)) {
                string ruleKey = section.Header.FirstOrDefault();
                if (String.IsNullOrEmpty(ruleKey)) {
                    Log.WriteLineWarning($"Unable to find rule key on line {section.LineNumber}.");
                    continue;
                }

                string ruleValue = section.Body.FirstOrDefault();
                if (String.IsNullOrEmpty(ruleValue)) {
                    Log.WriteLineWarning($"Unable to find rule value for rule {ruleKey} on line {section.LineNumber}.");
                    continue;
                }

                if (this.RuleExists(ruleKey)) {
                    this.SetRule(ruleKey, ruleValue);
                } else {
                    this.OverrideRule(ruleKey, ruleValue);
                }
            }
        }

        public bool RuleExists(string ruleKey) {
            return this.GetRule(ruleKey) != null;
        }

        public string GetRule(string ruleKey) {
            Debug.Assert(this.RuleExists(ruleKey), $"Unable to find rule definition for {ruleKey}");
            return this._rules[ruleKey];
        }

        private void SetRule(string ruleKey, string ruleValue) {
            Debug.Assert(!this.RuleExists(ruleKey), $"Unable to set the rule {ruleKey} which already exists.");
            Log.WriteLineVerbose($"Setting {ruleKey} rule as {ruleValue}");
            this._rules[ruleKey] = ruleValue;
        }

        private void OverrideRule(string ruleKey, string ruleValue) {
            Debug.Assert(this.RuleExists(ruleKey), $"Unable to override the rule {ruleKey} which does not exist.");
            Log.WriteLineWarning($"Changing {ruleKey} rule from {this.GetRule(ruleKey)} to {ruleValue}");
            this._rules[ruleKey] = ruleValue;
        }

        protected void SetRuleIfNotExists(string ruleKey, string ruleValue) {
            if (this.RuleExists(ruleKey)) {
                return;
            }
            this.SetRule(ruleKey, ruleValue);
        }
    }
}
