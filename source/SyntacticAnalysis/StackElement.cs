using LexicalAnalysis;
using System.Collections.Generic;
using System.Text;

namespace SyntacticAnalysis
{
    public abstract class StackElement
    {
    }

    public class StateStackElement : StackElement
    {
        private int _state;

        public StateStackElement(int stateNumber) {
            this._state = stateNumber;
        }

        public override string ToString() {
            return this._state.ToString();
        }

        public static implicit operator int(StateStackElement e) {
            return e._state;
        }
    }

    public class ASTNodeStackElement : StackElement
    {
        public ASTNode ASTNode;
        private string _displayString;

        public ASTNodeStackElement(Token token) {
            this.ASTNode = new ASTNode();
            this._displayString = token.TokenType;
            this.ASTNode.Elements["value"] = token.Content;
            this.ASTNode.Elements["column"] = token.Column;
            this.ASTNode.Elements["row"] = token.Row;
        }

        public ASTNodeStackElement(ASTNode node, string displayString) {
            this.ASTNode = node;
            this._displayString = displayString;
        }

        public override string ToString() {
            return this._displayString;
        }
    }

    public class ASTNode
    {
        public Dictionary<string, dynamic> Elements;

        public ASTNode() {
            this.Elements = new Dictionary<string, dynamic>();
        }

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
            if (this.Elements.ContainsKey(tag)) {
                if (this.Elements[tag] is List<ASTNode> lst) {
                    lst.Add(ast);
                } else {
                    // Re-Create the node as a list.
                    this.Elements[tag] = new List<ASTNode>() { this.Elements[tag], ast };
                }
            } else {
                this.Elements[tag] = ast;
            }
        }
    }

    public static class Extensions
    {
        public static string Multiply(this string value, int count) {
            var sb = new StringBuilder();
            for (int i = 0; i < count; i++) {
                sb.Append(value);
            }
            return sb.ToString();
        }
    }
}
