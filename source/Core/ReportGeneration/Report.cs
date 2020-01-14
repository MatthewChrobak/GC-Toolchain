using System.Linq;

namespace Core.ReportGeneration
{
    public class Report : ReportSection
    {
        public Report() : base("") {
        }

        public new string ToHTML() {
            // Step1: Generate table of contents.
            string bootstrapCSS = @"<link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/css/bootstrap.min.css' integrity='sha384-Vkoo8x4CGsO3+Hhxv8T/Q5PaXtkKtu6ug5TOeNV6gBiFeWPGFN9MuhOf23Q9Ifjh' crossorigin='anonymous'>";
            string visJs = @$"<script type=""text/javascript"" src=""https://unpkg.com/vis-network/standalone/umd/vis-network.min.js""></script>";

            string head = $"<head>{bootstrapCSS}{visJs}</head>";
            string tableOfContents = new TableOfContents(this).ToHTML();

            string body = $"<body>{tableOfContents}{string.Join("", this.Sections.Select(Section => Section.ToHTML()))}</body>";
            string html = $"<!doctype HTML><html>{head}{body}</html>";  

            return html;
        }
    }
}
