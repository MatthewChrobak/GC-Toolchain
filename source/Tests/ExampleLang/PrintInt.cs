using Core;
using NUnit.Framework;

namespace Tests.Lang
{
    public class PrintInt
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
        public void print_int_succeed() {
            string program = @"int main() {
    print_int(1);
}";
            var results = new ExampleLangTest(program);
            results.SymbolTableExists("::global::print_int")
                .WithOneRow((Column.EntityType, EntityType.Variable))
                .WithColumn(Column.IsParameter, true)
                .WithColumn(Column.ParameterIndex, 0);
            results.SymbolTableExists("::global::main");
            results.SymbolTableExists("::global")
                .WithOneRow((Column.Name, "main"), (Column.EntityType, EntityType.Function))
                .WithColumn(Column.Type, "int");

            Assert.AreEqual("1", results.ProgramOutput);
        }

        [Test]
        public void print_int_override_fails() {
            string program = @"int print_int(int val) { }";
            var e = Assert.Catch<AssertionFailedException>(new TestDelegate(() => new ExampleLangTest(program)));
            AssertExceptionCause(e, "The function print_int at 1:5 is already defined.");
        }

        [Test]
        public void print_int_from_variable_succeed() {
            string program = @"int main() { int x = 10; print_int(x); }";
            var results = new ExampleLangTest(program);
            Assert.AreEqual("10", results.ProgramOutput);
        }
    }
}
