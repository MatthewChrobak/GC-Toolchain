using Core;
using NUnit.Framework;

namespace Tests.ExampleLang
{
    public class PrintInt : LangUnitTestSuite
    {
        private static void AssertExceptionCause(AssertionFailedException e, string expected) {
            string actual = e.Message;
            if (actual.Length - expected.Length < 0) {
                Assert.Fail($"length of '{actual}' is less than length of '{expected}'");
            }

            string end = actual[^expected.Length..];
            Assert.AreEqual(expected, end);
        }

        [Test]
        public void print_int_literal_succeed() {
            string program = @"void main() {
    print_int(1);
}";
            var results = Run(program);
            results.SymbolTableExists("::global::print_int")
                .WithOneRow((Column.EntityType, EntityType.Variable))
                .WithColumn(Column.IsParameter, true)
                .WithColumn(Column.ParameterIndex, 0);
            results.SymbolTableExists("::global::main");
            results.SymbolTableExists("::global")
                .WithOneRow((Column.Name, "main"), (Column.EntityType, EntityType.Function))
                .WithColumn(Column.ReturnType, "void");

            Assert.AreEqual("1", results.ProgramOutput);
        }

        [Test]
        public void print_int_variable_succeed() {
            string program = @"void main() {
    int x = 2;
    print_int(x);
}";
            var results = Run(program);
            Assert.AreEqual("2", results.ProgramOutput);
        }

        [Test]
        public void print_int_override_fails() {
            string program = @"void print_int(int val) { }";
            var e = Assert.Catch<AssertionFailedException>(new TestDelegate(() => Run(program)));
            AssertExceptionCause(e, "The function print_int at (1, 6) already exists.");
        }

        [Test]
        public void print_int_from_variable_succeed() {
            string program = @"void main() { int x = 10; print_int(x); }";
            var results = Run(program);
            Assert.AreEqual("10", results.ProgramOutput);
        }
    }
}
