using System.Collections.Generic;
using System.Text;

namespace ASTVisitor
{
    public class ASTNode : AssociativeArray
    {
        public string ToJSON(int indentation = 0) {
            var sb = new StringBuilder();

            string prefix = "\t".Multiply(indentation);
            sb.AppendLine("{");

            foreach (var element in this.Elements) {
                sb.Append($"\"{element.Key}\":");
                if (element.Value is ASTNode node) {
                    sb.Append(node.ToJSON(indentation + 1));
                } else if (element.Value is List<ASTNode> lst) {
                    sb.Append("[");

                    foreach (var lstElement in lst) {
                        sb.Append(lstElement.ToJSON());
                        sb.Append(",");
                    }

                    sb.Append("]");
                } else {
                    sb.Append($"\"{element.Value}\"");
                }
                sb.AppendLine(",");
            }

            sb.AppendLine(prefix + "}");
            return sb.ToString();
        }

        public void Add(string tag, ASTNode ast) {
            if (this.Contains(tag)) {
                if (this[tag] is List<ASTNode> lst) {
                    lst.Add(ast);
                } else {
                    // Re-Create the node as a list.
                    this[tag] = new List<ASTNode>() { this[tag], ast };
                }
            } else {
                this[tag] = ast;
            }
        }
    }
}
