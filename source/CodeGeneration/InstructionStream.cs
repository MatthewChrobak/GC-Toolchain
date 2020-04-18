using System.Collections.Generic;
using System.Text;

namespace CodeGeneration
{
    public class InstructionStream
    {
        private readonly List<string> _instructions;
        private string _tabLength = "";

        public InstructionStream() {
            this._instructions = new List<string>();
        }

        public void Append(string instruction) {
            if (this._instructions.Count == 0) {
                this.AppendLine("");
            }
            this._instructions[^1] = this._instructions[^1] + instruction;
        }

        public void IncrementTab(int increment) {
            if (increment > 0) {
                var sb = new StringBuilder(this._tabLength);
                for (int i = 0; i < increment; i++) {
                    sb.Append('\t');
                }
                this._tabLength = sb.ToString();
            } else if (increment < 0) {
                this._tabLength = this._tabLength[0..^(increment * -1)];
            }
        }

        public void AppendLine(string instruction) {
            this._instructions.Add(this._tabLength + instruction);
        }

        public void AppendLineNoIndent(string instruction) {
            this._instructions.Add(instruction);
        }

        public int CreateEmptyInstruction() {
            this.AppendLine("");
            return this._instructions.Count - 1;
        }

        public void AppendInstructionAt(int index, string value) {
            this._instructions[index] = this._instructions[index] + value;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            foreach (string line in this._instructions) {
                sb.AppendLine(line);
            }
            return sb.ToString();
        }
    }
}
