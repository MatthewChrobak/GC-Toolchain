using System;
using System.IO;
using System.Linq;

namespace Core.ReportGeneration
{
    public class Report : ReportSection
    {
        private string reportPath;

        public Report(string reportPath) : base("") {
            this.reportPath = reportPath;
        }

        public new string ToHTML() {
            // Step1: Generate table of contents.
            string bootstrapCSS = @"<link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/css/bootstrap.min.css' integrity='sha384-Vkoo8x4CGsO3+Hhxv8T/Q5PaXtkKtu6ug5TOeNV6gBiFeWPGFN9MuhOf23Q9Ifjh' crossorigin='anonymous'>";
            string visJs = @$"<script type=""text/javascript"" src=""https://unpkg.com/vis-network/standalone/umd/vis-network.min.js""></script>";
            string jsonTreeView = @"<script src=""https://ajax.googleapis.com/ajax/libs/jquery/2.1.1/jquery.min.js""></script><script src=""https://rawgit.com/abodelot/jquery.json-viewer/master/json-viewer/jquery.json-viewer.js""></script>
<link href=""https://rawgit.com/abodelot/jquery.json-viewer/master/json-viewer/jquery.json-viewer.css"" rel=""stylesheet""/>";

            string head = $"<head>{bootstrapCSS}{visJs}{jsonTreeView}</head>";
            string tableOfContents = new TableOfContents(this).ToHTML();
            string backtotop = "<div style='position:fixed;top:0px;left:0px;padding:10px;'><a href='#' style='display:block;border:1px solid black;border-radius:10px;padding:10px;' onclick=\"$('html,body').animate({scrollTop:0}, 'fast');\">Back to top</a></div>";
            backtotop = "<div style='position:fixed;top:0px;left:0px;padding:10px;'><button type='button' class='btn btn-light' onclick=\"$('html,body').animate({scrollTop:0}, 'fast');\">Scroll to top</button></div>";

            string body = $"<body>{tableOfContents}{string.Join("", this.Sections.Select(Section => Section.ToHTML()))}{backtotop}</body>";
            string html = $"<!doctype HTML><html>{head}{body}</html>";  

            return html;
        }

        public void Save() {
            Log.WriteLineVerbose("Creating report...");
            File.WriteAllText(this.reportPath, this.ToHTML());
        }
    }
}
