using NUnit.Framework;

namespace Tests.ExampleLang
{
    public class VariableTests : LangUnitTestSuite
    {
        [Test]
        public void LiteralAssignment() {
            string program = @"void main() {
    int x = 10;
    int y = 20;

    print_int(x);
    print_int(y);
}";
            var results = new ExampleLangTest(program);
            Assert.AreEqual("1020", results.ProgramOutput);
        }

        [Test]
        public void VariableAssignment() {
            string program = @"void main() {
    int x = 10;
    int y = x;

    print_int(x);
    print_int(y);
}";
            var results = new ExampleLangTest(program);
            Assert.AreEqual("1010", results.ProgramOutput);
        }

        [Test]
        public void VariableAssignment_FollowedByLiteralAssignment() {
            string program = @"void main() {
    int x = 10;
    int y = x;
    x = 20;

    print_int(x);
    print_int(y);
}";
            var results = new ExampleLangTest(program);
            Assert.AreEqual("2010", results.ProgramOutput);
        }
    }
}
