using System;
using System.Linq;

namespace Core.LexicalAnalysis
{
    // TODO: Does this need to be here?
    public class TextFileScanner : IDisposable
    {
        private string[] _contents;

        public int Line { get; private set; }
        public int Column { get; private set; }
        public const char EOF = '\0';

        private string CurrentLine => this._contents[this.Line];
        private char CurrentChar => this.Column < this.CurrentLine.Length ? this.CurrentLine[this.Column] : EOF;
        public bool CanRead => this.Line < this._contents.Length && this.Column < this.CurrentLine.Length;

        public TextFileScanner(string content) {
            this.Line = 0;
            this.Column = 0;
            this._contents = content.Split("\r\n").Select(line => $"{line}\r\n").ToArray();
        }

        public TextFileScanner(string[] lines) {
            this.Line = 0;
            this.Column = 0;
            this._contents = lines;
        }

        public char Read() {
            char c = Peek();
            GoNext();
            return c;
        }

        public char Peek() {
            return this.CurrentChar;
        }

        public void GoNext() {
            this.Column++;
            if (this.Column == this.CurrentLine.Length) {
                this.Column = 0;
                this.Line++;
            }
            if (this.Line == this._contents.Length) {
                this.Line--;
                this.Column = this.CurrentLine.Length;
            }
        }

        public void GoPrevious() {
            this.Column--;
            if (this.Column == -1) {
                this.Line -= 1;
                this.Column = this.CurrentLine.Length - 1;
            }
        }

        public void GoToEnd() {
            this.Line = this._contents.Length - 1;
            this.Column = this.CurrentLine.Length;
        }

        public void GoToStart() {
            this.Line = 0;
            this.Column = 0;
        }

        public void Dispose() {

        }
    }
}