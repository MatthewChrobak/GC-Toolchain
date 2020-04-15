using NUnit.Framework;

namespace Tests.ExampleLang
{
    public class STDIOTests : LangUnitTestSuite
    {
        [Test]
        public void PrintInt_Exists() {
            string program = @"void main() { }";
            var results = Run(program);

            results.SymbolTableExists("::global::print_int")
                .WithOneRow(("is_parameter", true))
                .WithColumn("parameter_index", 0)
                .WithColumn("type", "int");
        }

        [Test]
        public void PrintInt_FromLiteral() {
            string program = @"void main() { print_int(1); }";
            var results = Run(program);
            Assert.AreEqual("1", results.ProgramOutput);
        }

        [Test]
        public void PrintInt_FromVariable() {
            string program = @"void main() { int x = 1; print_int(x); }";
            var results = Run(program);
            Assert.AreEqual("1", results.ProgramOutput);
        }
    }
}
