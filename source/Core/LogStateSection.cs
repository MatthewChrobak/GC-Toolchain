using Core.ReportGeneration;
using System.Collections.Generic;

namespace Core
{
    internal class LogStateSection : Section
    {
        private List<string> _lines;

        public LogStateSection(string key, List<string> lines) : base(key) {
            this._lines = lines;
        }

        public override string ToHTML() {
            if (this._lines.Count == 0) {
                return string.Empty;
            }
            return HeaderHTML + string.Join("<br>", this._lines);
        }
    }
}