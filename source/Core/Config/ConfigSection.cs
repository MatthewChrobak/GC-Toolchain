using System.Text.RegularExpressions;

namespace Core.Config
{
    public class ConfigSection
    {
        public readonly string ConfigFileName;
        public readonly int LineNumber;
        public readonly string Tag;
        public readonly string[] Header;
        public readonly string[] Body;

        public ConfigSection(string headerLine, string[] body, string configFileName, int lineNumber) {
            this.ConfigFileName = configFileName;
            this.LineNumber = lineNumber;
            
            Debug.Assert(headerLine.StartsWith(ConfigurationFile.SectionSymbol), $"Section header line on line {this.LineNumber} in {this.ConfigFileName} must start with '{ConfigurationFile.SectionSymbol}'");
            Debug.Assert(headerLine.Length > ConfigurationFile.SectionSymbol.Length, $"Section header line on line {this.LineNumber} in {this.ConfigFileName} must define a section tag");

            var splitHeaderLine = Regex.Split(headerLine, @"\s+");
            this.Tag = splitHeaderLine[0][1..];
            this.Header = splitHeaderLine[1..];
            this.Body = body;
        }

        public string GetLocation() {
            return $"[{this.ConfigFileName}:{this.LineNumber}]";
        }
    }
}
