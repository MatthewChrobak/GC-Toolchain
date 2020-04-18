using NUnit.Framework;

namespace Tests.ExampleLang
{
    public class IfStatementTests : LangUnitTestSuite
    {
        [Test]
        public void Fibonacci_WithElse() {
            string program = @"int Fib(int a) {
    if (a <= 1) {
        return a;
    } else {
        return Fib(a - 1) +  Fib(a - 2);
    }
}

void main() {
    print_int(Fib(12));
}";
            var results = Run(program);
            Assert.AreEqual("144", results.ProgramOutput);
        }

        [Test]
        public void Fibonacci_WithoutElse() {
            string program = @"int Fib(int a) {
    if (a <= 1) {
        return a;
    }
    return Fib(a - 1) +  Fib(a - 2);
}

void main() {
    print_int(Fib(12));
}";
            var results = Run(program);
            Assert.AreEqual("144", results.ProgramOutput);
        }
    }
}
