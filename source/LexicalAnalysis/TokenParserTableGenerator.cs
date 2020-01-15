using Automata;
using Automata.NonDeterministic;
using Core;
using Core.ReportGeneration;
using LexicalAnalysis.Automata;
using LexicalAnalysis.ReportGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LexicalAnalysis
{
    public class TokenParserTableGenerator
    {
        public readonly LexicalConfigurationFile ConfigurationFile;
        public readonly NFATable NFATable;

        private ReportSection _section = new ParentReportSection("State Machines");
        private ReportSection _subtokens_section = new ParentReportSection("SubTokens");
        private ReportSection _tokens_section = new ParentReportSection("Tokens");
        private Dictionary<string, ReportSection> _token_parts = new Dictionary<string, ReportSection>();

        public TokenParserTableGenerator(LexicalConfigurationFile configurationFile) {
            this.ConfigurationFile = configurationFile;
            this.NFATable = this.ConstructLexerTable();

            this._section.AddSection(this._subtokens_section);
            this._section.AddSection(this._tokens_section);
            foreach (var entry in this._token_parts.OrderBy(val => val.Key)) {
                this._tokens_section.AddSection(entry.Value);
            }
        }

        private NFATable ConstructLexerTable() {
            Log.WriteLineVerbose("Constructing subtokens...");
            var definedSubTokens = new Dictionary<string, TokenNFATable>();
            this.GetTokenDefinitions(this.ConfigurationFile.GetSections(LexicalConfigurationFile.SECTION_TAG_SUBTOKEN), definedSubTokens, definedSubTokens);

            foreach (var entry in definedSubTokens) {
                this._subtokens_section.AddSection(new AutomataVisualizationSection(entry.Key, entry.Value));
            }

            Log.WriteLineVerbose("Constructing tokens...");
            var definedTokens = new Dictionary<string, TokenNFATable>();
            this.GetTokenDefinitions(this.ConfigurationFile.GetSections(LexicalConfigurationFile.SECTION_TAG_TOKEN), definedTokens, definedSubTokens);

            foreach (var entry in definedTokens) {
                this._token_parts.Add(entry.Key, new ParentReportSection(entry.Key));
                this._token_parts[entry.Key].AddSection(new AutomataVisualizationSection("E-NFA", entry.Value) { IncludeInTableOfContents = false });
            }

            Log.WriteLineVerbose("Minimizing tokens...");
            var minimizedTokens = this.GetMinimizedTokens(definedTokens);

            Log.WriteLineVerbose("Constructing final NFA...");
            var finalNFA = new TokenNFATable();
            var start = finalNFA.CreateNode();
            finalNFA.StartState = start;

            foreach (var token in minimizedTokens) {
                finalNFA.InsertTableTransition(start, token);
            }

            this._section.AddSectionToTop(new AutomataVisualizationSection("Final NFA Parser", finalNFA));

            return finalNFA;
        }

        private IEnumerable<TokenNFATable> GetMinimizedTokens(Dictionary<string, TokenNFATable> definedTokens) {
            var tokenPriorities = this.GetTokenPriorities(this.ConfigurationFile.GetSections(LexicalConfigurationFile.SECTION_TAG_TOKEN));

            foreach (var entry in definedTokens) {
                string tokenName = entry.Key;
                var definedToken = entry.Value;

                definedToken.EndState.IsFinal = true;
                definedToken.EndState.SetTag(Node.LABEL, $"{tokenName}~{tokenPriorities[tokenName]}");

                var section = this._token_parts[entry.Key];

                // It's much faster to convert the rules to DFA's and minimize them in isolation rather than 
                // add them into the same NFA and then convert/minimize since they tend to deal with different symbols.

                Log.WriteLineVerbose($"Computing e-closure and NFA-To-DFA conversion for token {tokenName}...");
                var dfa = definedToken.ToDFATable();
                section.AddSection(new AutomataVisualizationSection("DFA", dfa) { IncludeInTableOfContents = false });

                Log.WriteLineVerbose($"Minimizing the dfa for token {tokenName}...");
                var minDFA = dfa.Minimize();
                section.AddSection(new AutomataVisualizationSection("Min-DFA", minDFA) { IncludeInTableOfContents = false });

                // Re-construct it as an NFA by removing trap states.
                Log.WriteLineVerbose($"Removing trap states for token {tokenName}...");
                var minNFA = new TokenNFATable(minDFA.RemoveTrapStates());
                section.AddSection(new AutomataVisualizationSection("Min-NFA", minNFA) { IncludeInTableOfContents = false });

                yield return minNFA;
            }
        }

        public ReportSection GetReportSections() {
            return this._section;
        }

        private Dictionary<string, int> GetTokenPriorities(IEnumerable<Core.Config.ConfigSection> sections) {
            var priorities = new Dictionary<string, int>();

            foreach (var section in sections) {
                Debug.Assert(section.Tag == LexicalConfigurationFile.SECTION_TAG_TOKEN, $"Unable to get priority from section in {section.GetLocation()}.");

                string tokenName = section.Header.FirstOrDefault();
                Debug.Assert(!String.IsNullOrEmpty(tokenName), $"Section tag for section in {section.GetLocation()} cannot be empty.");

                var priorityStrs = section.Header.Where(entry => entry.StartsWith(LexicalConfigurationFile.HEADER_PRIORITY_PREFIX));
                if (priorityStrs.Count() > 1) {
                    Log.WriteLineWarning($"Found multiple priorities for token {tokenName} in {section.GetLocation()}.");
                }
                string priorityStr = priorityStrs.FirstOrDefault();
                if (String.IsNullOrEmpty(priorityStr)) {
                    priorityStr = "0";
                } else {
                    priorityStr = priorityStr.Remove(0, LexicalConfigurationFile.HEADER_PRIORITY_PREFIX.Length);
                }

                if (!Int32.TryParse(priorityStr, out int priority)) {
                    Log.WriteLineError($"Unable to cast {priorityStr} to priority in {section.GetLocation()}.");
                }

                priorities[tokenName] = priority;
            }

            return priorities;
        }

        private void GetTokenDefinitions(IEnumerable<Core.Config.ConfigSection> sections, Dictionary<string, TokenNFATable> destination, Dictionary<string, TokenNFATable> support) {
            Debug.Assert(!sections.Any(section => section.Header.FirstOrDefault() == LexicalConfigurationFile.SECTION_TAG_SUBTOKEN || section.Header.FirstOrDefault() == LexicalConfigurationFile.SECTION_TAG_TOKEN), $"Unable to construct subtoken definition from a section with the tags: " + $"{String.Join("\r\n", sections.Where(section => section.Tag != LexicalConfigurationFile.SECTION_TAG_SUBTOKEN).Select(section => section.GetLocation() + "=>" + section.Tag))}");

            var pendingTokens = sections.ToHashSet();
            int passNumber = 1;
            while (pendingTokens.Count != 0) {
                Log.WriteLineVerbose($"Starting pass {passNumber++}");
                var sectionsDefinedInThisIteration = new HashSet<Core.Config.ConfigSection>();
                foreach (var section in pendingTokens) {
                    Log.WriteLineVerbose($"Trying to construct {section.Header.FirstOrDefault()} in {section.GetLocation()}...");
                    if (ConstructTokenDefinition(section, destination, support)) {
                        Log.WriteLineVerbose($"Constructed {section.Header.FirstOrDefault()} in {section.GetLocation()}.");
                        sectionsDefinedInThisIteration.Add(section);
                    }
                }

                foreach (var section in sectionsDefinedInThisIteration) {
                    pendingTokens.Remove(section);
                }

                Debug.Assert(sectionsDefinedInThisIteration.Count != 0, $"Language definition is not regular. {String.Join(", ", pendingTokens.Select(section => section.Header.FirstOrDefault()))} cannot be constructed.");
            }
        }

        private bool ConstructTokenDefinition(Core.Config.ConfigSection section, Dictionary<string, TokenNFATable> destination, Dictionary<string, TokenNFATable> support) {
            Debug.Assert(!String.IsNullOrEmpty(section.Header.FirstOrDefault()), $"Token Definition {section.GetLocation()} cannot have empty tag.");

            string tokenName = section.Header.FirstOrDefault();
            var finalRule = new TokenNFATable();
            var partialRules = new List<TokenNFATable>();

            foreach (var rule in section.Body) {
                if (String.IsNullOrEmpty(rule)) {
                    continue;
                }

                var ruleComponents = Regex.Split(rule, @"\s+");
                var partialRule = new TokenNFATable();
                var ptr = partialRule.CreateNode();
                partialRule.StartState = ptr;

                for (int componentIndex = 0; componentIndex < ruleComponents.Length; componentIndex++) {
                    var component = ruleComponents[componentIndex];

                    // Zero or more?
                    Node? zero_or_more_loopPoint = null;
                    if (component.EndsWith(this.ConfigurationFile.GetRule(LexicalConfigurationFile.RULE_ZERO_OR_MORE_KEY))) {
                        zero_or_more_loopPoint = ptr;
                        component = component[..^1];

                        // What if it's prefaced with a literal?
                        if (component.EndsWith(this.ConfigurationFile.GetRule(LexicalConfigurationFile.RULE_LITERAL_PREFIX_KEY))) {
                            // Undo what we removed.
                            component += this.ConfigurationFile.GetRule(LexicalConfigurationFile.RULE_LITERAL_PREFIX_KEY);
                            zero_or_more_loopPoint = null;
                        }
                    }

                    // Hex literal?
                    if (component.StartsWith(this.ConfigurationFile.GetRule(LexicalConfigurationFile.RULE_HEX_PREFIX_KEY))) {
                        component = component[1..];
                        char symbol = (char)Int16.Parse(component, System.Globalization.NumberStyles.HexNumber);
                        component = $"{this.ConfigurationFile.GetRule(LexicalConfigurationFile.RULE_LITERAL_PREFIX_KEY)}{symbol}";
                    }

                    // Token?
                    if (component.StartsWith(this.ConfigurationFile.GetRule(LexicalConfigurationFile.RULE_TOKEN_PREFIX_KEY))) {
                        component = component[1..];
                        if (!support.ContainsKey(component)) {
                            Log.WriteLineVerbose($"Unable to define '{tokenName}' since it depends on '{component}' which has not yet been defined.");
                            return false;
                        }
                        ptr = partialRule.InsertTableTransition_AndCreateAnEndState(ptr, support[component]);
                    } else {
                        // Nope.

                        // Is it a range?
                        if (component.Length == 3 &&
                            component[0] != ConfigurationFile.GetRule(LexicalConfigurationFile.RULE_HEX_PREFIX_KEY) &&
                            component[1] == ConfigurationFile.GetRule(LexicalConfigurationFile.RULE_RANGE_INCLUSIVE_KEY)) {

                            var next = partialRule.CreateNode();
                            var lb = component[0];
                            var ub = component[2];

                            for (char i = lb; i <= ub; i++) {
                                partialRule.AddTransition(ptr, next, $"{i}");
                            }

                            // This will also work with loopback since the origin is saved.
                            ptr = next;
                        } else {
                            // Nope it's a literal.
                            for (int i = 0; i < component.Length; i++) {
                                if (component[i] == ConfigurationFile.GetRule(LexicalConfigurationFile.RULE_LITERAL_PREFIX_KEY)) {
                                    i++;
                                    if (i >= component.Length) {
                                        break;
                                    }
                                }
                                var next = partialRule.CreateNode();
                                partialRule.AddTransition(ptr, next, $"{component[i]}");
                                ptr = next;
                            }
                        }
                    }

                    // Do we loop back?
                    if (zero_or_more_loopPoint != null) {
                        partialRule.AddTransition(ptr, zero_or_more_loopPoint);
                        partialRule.AddTransition(zero_or_more_loopPoint, ptr);
                    }
                }

                partialRules.Add(partialRule);
            }

            var start = finalRule.CreateNode();
            var ends = new List<Node>();
            finalRule.StartState = start;

            foreach (var partialRule in partialRules) {
                var ruleEnd = finalRule.InsertTableTransition_AndCreateAnEndState(start, partialRule);
                ends.Add(ruleEnd);
            }

            var actualEnd = finalRule.CreateNode();
            foreach (var end in ends) {
                finalRule.AddTransition(end, actualEnd);
            }

            destination.Add(tokenName, finalRule);
            return true;
        }
    }
}
