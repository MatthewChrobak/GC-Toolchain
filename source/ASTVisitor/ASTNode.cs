using System;
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

                    // TODO: Visual representation on the report seems to be reversed.
                    // Actual pre-order / post-order ordering for traversal seems to be fine.
                    for (int ii = lst.Count - 1; ii >= 0; ii--) {
                        var lstElement = lst[ii];
                        sb.Append(lstElement.ToJSON());
                        sb.Append(",");
                    }

                    sb.Append("]");
                } else {
                    sb.Append($"\"{Escape(element.Value.ToString())}\"");
                }
                sb.AppendLine(",");
            }

            sb.AppendLine(prefix + "}");
            return sb.ToString();
        }

        private string Escape(string value) {
            value = value.Replace("\r", "/r");
            value = value.Replace("\"", "''");
            value = value.Replace("\n", "/n");
            return value;
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
