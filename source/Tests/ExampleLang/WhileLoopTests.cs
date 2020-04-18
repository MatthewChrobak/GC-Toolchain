using NUnit.Framework;

namespace Tests.ExampleLang
{
    public class WhileLoopTests : LangUnitTestSuite
    {
        [Test]
        public void WhileLoop_IsSecondStatement() {
            string program = @"void main() {
    int x = 10;
    while (x >= 0) {
        print_int(x);
        x = x - 1;
    }
}";
            var results = Run(program);
            Assert.AreEqual("109876543210", results.ProgramOutput);
        }

        [Test]
        public void WhileLoop_IsFirstStatement() {
            string program = @"void main() {
    while (5 < 3) {
        print_int(1);
    }
    print_int(2);
}";
            var results = Run(program);
            Assert.AreEqual("2", results.ProgramOutput);
        }
    }
}
