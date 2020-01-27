using Core.ReportGeneration;

namespace GCT
{
    public class ASTViewer : ReportSection
    {
        private string json;
        private const string id = "ast-json-view";

        public ASTViewer(string json) : base("AST-Tree") {
            this.json = json;
        }

        public override string GetContent() {
            return $@"
<pre id={id}></pre>
<script>
var data = {json}

$(function() {{
    $('#{id}').jsonViewer(data);
}});
</script>
";
        }
    }
}