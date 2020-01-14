using System.Collections.Generic;
using System.Linq;

namespace Core.ReportGeneration
{
    internal class TableOfContents : ReportSection
    {
        private List<ReportSection> _sections;

        public TableOfContents(Report report) : base("Table of Contents") {
            report.RefreshSectionNumber("");

            _sections = new List<ReportSection>();
            var stk = new Stack<ReportSection>();

            foreach (var section in report.Sections) {
                stk.Push(section);
            }

            while (stk.Count != 0) {
                var section = stk.Pop();
                _sections.Add(section);
                foreach (var s in section.Sections) {
                    stk.Push(s);
                }
            }
        }

        public override string GetContent() {
            return string.Join("<br>", this._sections.Select(section => $"<a href='#{section.SectionNumber}\t{section.Header}'>{section.SectionNumber}\t{section.Header}</a>").OrderBy(val => val));
        }
    }
}