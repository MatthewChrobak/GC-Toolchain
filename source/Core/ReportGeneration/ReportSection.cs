using System.Collections.Generic;
using System.Linq;

namespace Core.ReportGeneration
{
    public abstract class ReportSection
    {
        public string? Header { get; set; }
        public readonly List<ReportSection> Sections = new List<ReportSection>();
        public string SectionNumber { get; private set; } = System.String.Empty;
        private int LocalSectionNumber;
        public string Content { get; set; }
        public bool IncludeInTableOfContents { get; set; } = true;

        public string ID_HTML => $"{this.SectionNumber}\t{this.Header}";
        public string HeaderHTML => $"<h1 id='{ID_HTML}'>{ID_HTML}</h1>";


        public ReportSection(string header) {
            this.Header = header;
            this.Content = System.String.Empty;
        }

        public void AddSection(ReportSection section) {
            this.Sections.Add(section);
        }

        public IEnumerable<ReportSection> GetOrderedSections() {
            return this.Sections.Where(section => section.IncludeInTableOfContents).OrderByDescending(section => section.LocalSectionNumber);
        }

        public void AddSectionToTop(ReportSection section) {
            this.Sections.Insert(0, section);
        }

        public void RefreshSectionNumber(string prefix) {
            this.SectionNumber = prefix;
            this.LocalSectionNumber = 0;
            if (prefix.Length != 0) {
                this.LocalSectionNumber = int.Parse(prefix[(prefix.LastIndexOf('.') + 1)..]);
            }

            if (prefix.Length == 0) {
                for (int i = 1; i <= this.Sections.Count; i++) {
                    this.Sections[i - 1].RefreshSectionNumber($"{i}");
                }
            } else {
                for (int i = 1; i <= this.Sections.Count; i++) {
                    this.Sections[i - 1].RefreshSectionNumber($"{prefix}.{i}");
                }
            }
        }

        public virtual string GetContent() {
            return $"<div class='row'>{this.Content}</div>";
        }

        public string ToHTML() {
            return $"<div class='container' style='margin-top:100px'><div class='col-sm-12'>" +
                this.HeaderHTML +
                $"<div class='container'>{this.GetContent()}</div>" +
                $"</div></div>";
        }
    }
}
