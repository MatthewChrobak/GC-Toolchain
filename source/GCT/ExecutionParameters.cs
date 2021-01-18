using Core.ReportGeneration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GCT
{
    public partial class ExecutionParameters
    {
        private const string CurrentWorkingDirectoryFlag = "f";
        private string CurrentWorkingDirectory => Get(CurrentWorkingDirectoryFlag, AppDomain.CurrentDomain.BaseDirectory)!;

        private readonly Dictionary<string, object?> _parameters;
        
        public ExecutionParameters(string[] executionArgs) : this(string.Join(' ', executionArgs)) {
        }

        private ExecutionParameters(string executionInput) {
            this._parameters = new Dictionary<string, object?>();

            // Set CWD. If we overwrite it layer, then we overwrite it later.
            this.SetParameter(CurrentWorkingDirectoryFlag, AppDomain.CurrentDomain.BaseDirectory);

            var matches = Regex.Matches(executionInput, this.GetFlagRegex());
            for (int i = 0; i < matches.Count; i++) {
                var match = matches[i];
                string flag = match.Groups[1].Value;
                string value = match.Groups[2].Value;
                this.SetParameter(flag, value);
            }
        }

        public string GetFilePath(string key, string? placeholder) {
            string path = GetCWDRelativePath(key, placeholder);
            if (!File.Exists(path)) {
                using var _ = File.Create(path);
            }
            return path;
        }

        public string GetCWDRelativePath(string key, string? placeholder) {
            return Path.Combine(CurrentWorkingDirectory, this.Get(key, placeholder)!);
        }

        public string GetFolderPath(string key, string? placeholder) {
            string path = GetCWDRelativePath(key, placeholder);
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public string? Get(string key, string? placeholder) {
            if (this._parameters.ContainsKey(key)) {
                return this._parameters[key] as string;
            }
            return placeholder;
        }

        public bool? Get(string key, bool placeholder) {
            if (this._parameters.ContainsKey(key)) {
                return this._parameters[key] as bool?;
            }
            return placeholder;
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

            // If the value is empty, it must be a true bool. Otherwise, a string.
            if (value.Length == 0) {
                return true;
            } else {
                return value;
            }
        }

        private string GetFlagRegex() {
            string flag = @"-(\w+)";
            string flagValue_noquotes = @"(\s+[^\r\n\s\-]+)";
            string flagValue_singlequotes = @"(\s+\'[^\r\n\']+\')";
            string flagValue_doublequotes = @"(\s+\""[^\r\n\""]+\"")";
            return $"{flag}({flagValue_doublequotes}|{flagValue_singlequotes}|{flagValue_noquotes})?";
        }
    }

    public partial class ExecutionParameters : IReportable
    {
        public IEnumerable<ReportSection> GetReportSections() {
            yield return new ParametersSection(this._parameters);
        }

        private class ParametersSection : ReportSection
        {
            private readonly Dictionary<string, object?> _parameters;

            public ParametersSection(Dictionary<string, object?> parameters) : base("Execution-Parameters") {
                this._parameters = parameters;
            }

            public override string GetContent() {
                var sb = new StringBuilder();
                sb.Append(@"
<div>
    <table class='table'>
        <tr>
            <th scope='col'>Parameter</th>
            <th scope='col'>Value</th>
        <tr>");
                foreach (var row in this._parameters) {
                    sb.Append(@$"
        <tr>
            <td>{row.Key}</td>
            <td>{row.Value}</td>
        </tr>
");
                }
                sb.Append(@"
    </table>
</div>
");
                return sb.ToString();
            }
        }
    }
}
