using Core.ReportGeneration;
using System.Collections.Generic;

namespace Core
{
    internal class LogStateSection : ReportSection
    {
        private List<string> _lines;

        public LogStateSection(string key, List<string> lines) : base(key) {
            this._lines = lines;
        }

        public override string GetContent() {
            if (this._lines.Count == 0) {
                return string.Empty;
            }
            return string.Join("<br>", this._lines);
        }
    }
}