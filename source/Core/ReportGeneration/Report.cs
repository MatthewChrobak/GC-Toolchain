using System.Linq;

namespace Core.ReportGeneration
{
    public class Report : Section
    {
        public Report() : base("") {
        }

        public override string ToHTML() {
            // Step1: Generate table of contents.
            string bootstrapCSS = @"<link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/css/bootstrap.min.css' integrity='sha384-Vkoo8x4CGsO3+Hhxv8T/Q5PaXtkKtu6ug5TOeNV6gBiFeWPGFN9MuhOf23Q9Ifjh' crossorigin='anonymous'>";
            string head = $"<head>{bootstrapCSS}</head>";
            string tableOfContents = new TableOfContents(this).ToHTML();

            string body = $"<body>{tableOfContents}<hr>{string.Join("", this.Sections.Select(Section => Section.ToHTML()))}</body>";
            string html = $"<!doctype HTML><html>{head}{body}</html>";

            return html;
        }
    }
}
