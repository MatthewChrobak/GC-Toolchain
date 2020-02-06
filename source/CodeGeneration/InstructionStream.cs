using System.Text;

namespace CodeGeneration
{
    public class InstructionStream
    {
        private StringBuilder _instructions;

        public InstructionStream() {
            this._instructions = new StringBuilder();
        }

        public void Append(string instruction) {
            this._instructions.Append(instruction);
        }

        public void AppendLine(string instruction) {
            this._instructions.AppendLine(instruction);
        }

        public override string ToString() {
            return this._instructions.ToString();
        }
    }
}
