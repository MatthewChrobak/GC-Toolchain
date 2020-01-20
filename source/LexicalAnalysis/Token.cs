namespace LexicalAnalysis
{
    public class Token
    {
        public readonly string TokenType;
        public readonly string Content;
        public readonly int Row;
        public readonly int Column;

        public Token(string tokenType, string content, int row, int column) {
            this.TokenType = tokenType;
            this.Content = content;
            this.Row = row;
            this.Column = column;
        }

        public override string ToString() {
            return $"[{this.TokenType} at {this.Row}:{this.Column} = {this.Content}]";
        }
    }
}