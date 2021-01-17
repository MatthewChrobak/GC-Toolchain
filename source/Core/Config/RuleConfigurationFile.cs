﻿using Core.Logging;
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
        private readonly Log? _log;

        public RuleConfigurationFile(string configurationFilePath, Log? log) : this(File.ReadAllLines(configurationFilePath), new FileInfo(configurationFilePath).Name, log) {
        }

        public RuleConfigurationFile(string[] configurationFileContents, string configurationFileName, Log? log) : base(configurationFileContents, configurationFileName) {
            this._log = log;

            foreach (var section in this.GetSections(SECTION_TAG_RULE)) {
                string ruleKey = section.Header.FirstOrDefault();
                if (String.IsNullOrEmpty(ruleKey)) {
                    this._log?.WriteLineWarning($"Unable to find rule key in {section.GetLocation()}.");
                    continue;
                }

                string ruleValueString = section.Body.FirstOrDefault();
                if (String.IsNullOrEmpty(ruleValueString)) {
                    this._log?.WriteLineWarning($"Unable to find rule value for rule {ruleKey} in {section.GetLocation()}.");
                    continue;
                }
                if (ruleValueString.Length != 1) {
                    this._log?.WriteLineWarning($"Rule values can only be one character long. Shortening the rule {ruleKey} from '{ruleValueString}' to '{ruleValueString[0]}'.");
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
            this._log?.WriteLineVerbose($"Setting {ruleKey} rule as {ruleValue}");
            VerifyNonDuplicateRuleValue(ruleValue);
            this._rules[ruleKey] = ruleValue;
        }

        private void VerifyNonDuplicateRuleValue(char ruleValue) {
            foreach (var rule in this._rules) {
                Debug.Assert(rule.Value != ruleValue, $"Rule {rule.Key} already uses the rule value '{ruleValue}'");
            }
        }

        private void OverrideRule(string ruleKey, char ruleValue) {
            Debug.Assert(this.RuleExists(ruleKey), $"Unable to override the rule {ruleKey} which does not exist");
            this._log?.WriteLineWarning($"Changing {ruleKey} rule from {this.GetRule(ruleKey)} to {ruleValue}");
            VerifyNonDuplicateRuleValue(ruleValue);
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
