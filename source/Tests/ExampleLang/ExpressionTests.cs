using NUnit.Framework;

namespace Tests.ExampleLang
{
    public class ExpressionTests : LangUnitTestSuite {
        [Test]
        public void Addition_Literals() {
            string program = @"void main() {
    print_int(5+10);
}";
            var results = Run(program);
            Assert.AreEqual("15", results.ProgramOutput);
        }

        [Test]
        public void Addition_Variables() {
            string program = @"void main() {
    int x = 10;
    int y = 5;
    print_int(x + y);
}";
            var results = Run(program);
            Assert.AreEqual("15", results.ProgramOutput);
        }

        [Test]
        public void Addition_LiteralVariable() {
            string program = @"void main() {
    int x = 10;
    print_int(x + 5);
}";
            var results = Run(program);
            Assert.AreEqual("15", results.ProgramOutput);
        }

        [Test]
        public void Subtraction_Literals() {
            string program = @"void main() {
    print_int(10 - 5);
}";
            var results = Run(program);
            Assert.AreEqual("5", results.ProgramOutput);
        }

        [Test]
        public void Subtraction_Variables() {
            string program = @"void main() {
    int x = 10;
    int y = 5;
    print_int(x - y);
}";
            var results = Run(program);
            Assert.AreEqual("5", results.ProgramOutput);
        }

        [Test]
        public void Subtraction_LiteralVariable() {
            string program = @"void main() {
    int x = 10;
    print_int(x - 5);
}";
            var results = Run(program);
            Assert.AreEqual("5", results.ProgramOutput);
        }

        [Test]
        public void Multiplication_Literals() {
            string program = @"void main() {
    print_int(5 * 10);
}";
            var results = Run(program);
            Assert.AreEqual("50", results.ProgramOutput);
        }

        [Test]
        public void Multiplication_Variables() {
            string program = @"void main() {
    int x = 10;
    int y = 5;
    print_int(x * y);
}";
            var results = Run(program);
            Assert.AreEqual("50", results.ProgramOutput);
        }

        [Test]
        public void Multiplication_LiteralVariable() {
            string program = @"void main() {
    int x = 10;
    print_int(x * 5);
}";
            var results = Run(program);
            Assert.AreEqual("50", results.ProgramOutput);
        }

        [Test]
        public void Division_Literals() {
            string program = @"void main() {
    print_int(10 / 5);
}";
            var results = Run(program);
            Assert.AreEqual("2", results.ProgramOutput);
        }

        [Test]
        public void Division_Variables() {
            string program = @"void main() {
    int x = 10;
    int y = 5;
    print_int(x / y);
}";
            var results = Run(program);
            Assert.AreEqual("2", results.ProgramOutput);
        }

        [Test]
        public void Division_LiteralVariable() {
            string program = @"void main() {
    int x = 10;
    print_int(x / 5);
}";
            var results = Run(program);
            Assert.AreEqual("2", results.ProgramOutput);
        }

        [Test]
        public void AllOperators_Literals() {
            string program = @"void main() {
    print_int(4 / 2 - 3 * 5 + 7);
}";
            var results = Run(program);
            Assert.AreEqual("-6", results.ProgramOutput);
        }

        [Test]
        public void AllOperators_Varaibles() {
            string program = @"void main() {
    int a = 4;
    int b = 2;
    int c = 3;
    int d = 5;
    int e = 7;
    print_int(a / b - c * d + e);
}";
            var results = Run(program);
            Assert.AreEqual("-6", results.ProgramOutput);
        }

        [Test]
        public void AllOperators_LiteralVaraibles() {
            string program = @"void main() {
    int a = 4;
    int b = 2;
    int c = 3;
    print_int(a / b - c * 5 + 7);
}";
            var results = Run(program);
            Assert.AreEqual("-6", results.ProgramOutput);
        }

        [Test]
        public void AllOperators_Parenthesis() {
            string program = @"void main() {
    int a = 4;
    int b = 2;
    int c = 3;
    int d = 6;
    int e = 9;
    print_int((a - b) * (c + d) / e);
}";
            var results = Run(program);
            Assert.AreEqual("2", results.ProgramOutput);
        }

        [Test]
        public void Sign_Literal() {
            string program = @"void main() {
    print_int(-5);
}";
            var results = Run(program);
            Assert.AreEqual("-5", results.ProgramOutput);
        }

        [Test]
        public void Sign_Variable() {
            string program = @"void main() {
    int x = 5;
    print_int(-(x));
}";
            var results = Run(program);
            Assert.AreEqual("-5", results.ProgramOutput);
        }

        [Test]
        public void Sign_In_Expression() {
            string program = @"int Test(int param1, int param2) {
    return ((param2 + param1) - (param2)) / 5;
}

void main() {
    print_int(Test(-5, -10));
}
";
            var results = Run(program);
            Assert.AreEqual("-1", results.ProgramOutput);
        }

        [Test]
        public void Sign_Outside_Expression() {
            string program = @"int Test(int param1, int param2) {
    return -((param2 + param1) - (param2)) / 5;
}

void main() {
    print_int(Test(-5, -10));
}
";
            var results = Run(program);
            Assert.AreEqual("1", results.ProgramOutput);
        }

        [Test]
        public void Comparison_Int() {
            string program = @"void cmp(int a, int b) {
    print_int(a > b);
    print_int(a >= b);
    print_int(a < b);
    print_int(a <= b);
    print_int(a == b);
    print_int(a != b);
}

void main() {
    cmp(5, 5);
    cmp(5, 0);
    cmp(0, 5);
}";
            var results = Run(program);
            Assert.AreEqual("010110110001001101", results.ProgramOutput);
        }

        [Test]
        public void LogicalOperators() {
            string program = @"void main() {
    print_int(5 && 15);
    print_int(1 || 7);
}";
            var results = Run(program);
            Assert.AreEqual("57", results.ProgramOutput);
        }

        [Test]
        public void OperatorPrecedence() {
            string program = @"void main() {
    print_int(5 + 3 * 10 == 1 + 170 / 5 && 1 + 5 <= 7);
}";
            var results = Run(program);
            Assert.AreEqual("1", results.ProgramOutput);
        }

        [Test]
        public void Sign_NestedInExpression() {
            string program = @"void main() {
    print_int(-5 + 5);
}";
            var results = Run(program);
            Assert.AreEqual("0", results.ProgramOutput);
        }
    }
}
