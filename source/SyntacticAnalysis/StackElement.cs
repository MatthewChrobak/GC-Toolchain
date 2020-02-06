using ASTVisitor;
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
            this.ASTNode["value"] = token.Content;
            this.ASTNode["column"] = token.Column;
            this.ASTNode["row"] = token.Row;
        }

        public ASTNodeStackElement(ASTNode node, string displayString) {
            this.ASTNode = node;
            this._displayString = displayString;
        }

        public override string ToString() {
            return this._displayString;
        }
    }
}
