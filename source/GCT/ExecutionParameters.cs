using Core.ReportGeneration;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GCT
{
    public partial class ExecutionParameters
    {
        public const string CurrentWorkingDirectory = "CWD";

        private readonly Dictionary<string, object?> _parameters;

        public object? this[string key] {
            get {
                if (!this._parameters.ContainsKey(key)) {
                    return null;
                }
                return this._parameters[key];
            }
        }

        public ExecutionParameters(string[] executionArgs) : this(string.Join(' ', executionArgs)) {
        }

        private ExecutionParameters(string executionInput) {
            this._parameters = new Dictionary<string, object?>();

            // Set CWD. If we overwrite it layer, then we overwrite it later.
            this.SetParameter(CurrentWorkingDirectory, AppDomain.CurrentDomain.BaseDirectory);

            var matches = Regex.Matches(executionInput, this.GetFlagRegex());
            for (int i = 0; i < matches.Count; i++) {
                var match = matches[i];
                string flag = match.Groups[1].Value;
                string value = match.Groups[2].Value;
                this.SetParameter(flag, value);
            }
        }

        private void SetParameter(string key, string value) {
            key = key.Trim();
            var interpretedValue = this.InterpretValue(value.Trim());
            this._parameters[key] = interpretedValue;
        }

        private object? InterpretValue(string value) {
            // Remove any layered quotes if they exist
            if (value.StartsAndEndsWith('\'') || value.StartsAndEndsWith('"')) {
                value = value[1..^1];
            }

            // Try and interpret as bool. If not, default to string
            if (bool.TryParse(value, out bool res)) {
                return res;
            } else {
                return value;
            }
        }

        private string GetFlagRegex() {
            string flag = @"-(\w+)";
            string flagValue_noquotes = @"(\s+[^\r\n\s\-]+)";
            string flagValue_singlequotes = @"(\s+\'[^\r\n\']\'+)";
            string flagValue_doublequotes = @"(\s+\""[^\r\n\""]+\"")";
            return $"{flag}({flagValue_noquotes}|{flagValue_singlequotes}|{flagValue_doublequotes})?";
        }
    }

    public partial class ExecutionParameters : IReportable
    {
        public IEnumerable<ReportSection> GetReportSections() {
            yield return new Test();
        }

        private class Test : ReportSection
        {
            public Test() : base("test") {

            }
        }
    }
}
