using Core.ReportGeneration;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    internal class LogSection : ReportSection
    {
        public LogSection(Dictionary<string, List<string>> stateHistory) : base("Log Entries") {
            foreach (var entry in stateHistory) {
                this.Sections.Add(new LogStateSection(entry.Key, entry.Value));
            }
        }

        public override string GetContent() {
            return string.Join("", this.Sections.Select(s => s.ToHTML()));
        }
    }
}