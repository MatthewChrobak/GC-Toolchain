using System.Linq;

namespace Core.ReportGeneration
{
    public class ParentReportSection : ReportSection
    {
        public ParentReportSection(string header) : base(header) {
        }

        public override string GetContent() {
            return string.Join("", this.Sections.Select(section => section.ToHTML()));
        }
    }
}
