using System.Text;

namespace CodeGeneration
{
    public class InstructionStream
    {
        private StringBuilder _instructions;
        private string _tabLength = "";

        public InstructionStream() {
            this._instructions = new StringBuilder();
        }

        public void Append(string instruction) {
            this._instructions.Append(instruction);
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
            this._instructions.AppendLine(this._tabLength + instruction);
        }

        public override string ToString() {
            return this._instructions.ToString();
        }
    }
}
