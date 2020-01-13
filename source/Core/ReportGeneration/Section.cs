using System.Collections.Generic;

namespace Core.ReportGeneration
{
    public abstract class Section
    {
        public string? Header { get; set; }
        public readonly List<Section> Sections = new List<Section>();
        public string SectionNumber { get; private set; } = System.String.Empty;
        public string Content { get; set; }

        public string ID_HTML => $"{this.SectionNumber}\t{this.Header}";
        public string HeaderHTML => $"<h1 id='{ID_HTML}'>{ID_HTML}</h1>";

        public Section(string header) {
            this.Header = header;
            this.Content = System.String.Empty;
        }

        public void AddSection(Section section) {
            this.Sections.Add(section);
        }

        public void AddSectionToTop(Section section) {
            this.Sections.Insert(0, section);
        }

        public void RefreshSectionNumber(string prefix) {
            this.SectionNumber = prefix;
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

        public virtual string ToHTML() {
            return $"<div class='container' id='{this.SectionNumber}' style='margin-top:100px'><div class='col-sm-12'>" +
                $"<h2>{this.Header}</h2>" +
                $"<div class='container'><div class='row'>{this.Content}</div></div>" +
                $"</div>"; ;
        }
    }
}
