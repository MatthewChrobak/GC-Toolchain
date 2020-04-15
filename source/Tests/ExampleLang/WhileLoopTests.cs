using NUnit.Framework;

namespace Tests.ExampleLang
{
    public class WhileLoopTests : LangUnitTestSuite
    {
        [Test]
        public void WhileLoop() {
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
    }
}
