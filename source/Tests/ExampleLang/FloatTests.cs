using NUnit.Framework;

namespace Tests.ExampleLang
{
    public class FloatTests : LangUnitTestSuite
    {
        [Test]
        public void FloatLiteral() {
            string program = @"void main() {
    print_float(1.0);
}";
            var results = Run(program);
            Assert.AreEqual("1.000000", results.ProgramOutput);
        }

        [Test]
        public void FloatVariable() {
            string program = @"void main() {
    float x = 1.0;
    print_float(x);
}";
            var results = Run(program);
            Assert.AreEqual("1.000000", results.ProgramOutput);
        }

        [Test]
        public void Zero() {
            string program = @"void main() {
    print_float(0.0);
}";
            var results = Run(program);
            Assert.AreEqual("0.000000", results.ProgramOutput);
        }

        [Test]
        public void DivisionByZero() {
            string program = @"void main() {
    print_float(5.0/0.0);
}";
            var results = Run(program);
            Assert.AreEqual("inf", results.ProgramOutput);
        }

        [Test]
        public void NegativeDivisionByZero() {
            string program = @"void main() {
    print_float(-5.0/0.0);
}";
            var results = Run(program);
            Assert.AreEqual("-inf", results.ProgramOutput);
        }

        [Test]
        public void Comparison_Float() {
            string program = @"void cmp(float a, float b) {
    print_int(a > b);
    print_int(a >= b);
    print_int(a < b);
    print_int(a <= b);
    print_int(a == b);
    print_int(a != b);
}

void main() {
    cmp(5.0, 5.0);
    cmp(5.0, 0.0);
    cmp(0.0, 5.0);
}";
            var results = Run(program);
            Assert.AreEqual("010110110001001101", results.ProgramOutput);

        }

        [Test]
        public void ReturnType_Float() {
            string program = @"float foo() {
    return 5.0;
}
void main() {
    print_float(foo());
}";
            var results = Run(program);
        }

        [Test]
        public void LogicalOperators_CannotBeFloat() {
            string program = @"void main() {
    if (5.0 && 6.0) {
    }
}";
            AssertExceptionCause(program, "If-statement condition at (1, 23) needs to be of type int.");
        }

        [Test]
        public void OperatorPrecedence() {
            string program = @"void main() {
    print_int(5.0 + 3.0 * 10.0 == 1.0 + 170.0 / 5.0 && 1.0 + 5.0 <= 7.0);
}";
            var results = Run(program);
            Assert.AreEqual("1", results.ProgramOutput);
        }

        [Test]
        public void Sign() {
            string program = @"void main() {
    print_float(-5.0);
}";
            var results = Run(program);
            Assert.AreEqual("-5.000000", results.ProgramOutput);
        }

        [Test]
        public void AssignFloatToInt() {
            string program = @"void main() {
    int x = 5.0;
}";
            AssertExceptionCause(program, "Unable to assign 'float' to 'int' at (1, 26).");
        }

        [Test]
        public void AssignIntToFloat() {
            string program = @"void main() {
    float x = 5;
}";
            AssertExceptionCause(program, "Unable to assign 'int' to 'float' at (1, 28).");
        }

        [Test]
        public void AssignFloatToInt_LValue() {
            string program = @"void main() {
    int x;
    x = 5.0;
}";
            AssertExceptionCause(program, "Unable to assign 'float' to 'int' at (1, 34).");
        }

        [Test]
        public void AssignIntToFloat_LValue() {
            string program = @"void main() {
    float x;
    x = 5;
}";
            AssertExceptionCause(program, "Unable to assign 'int' to 'float' at (1, 36).");
        }

        public void PrintParticularValue(string input, string output) {
            string program = $"void main() {{ print_float({input}); }}";
            var results = Run(program);
            Assert.AreEqual(output, results.ProgramOutput);
        }

        [Test]
        public void PrintParticularValues() {
            (string input, string output)[] tests = {
                ("17.52", "17.520000"),
                ("17.49999", "17.499990"),
                ("17.49399", "17.493990"),
                ("193204.30943009", "193204.312500"),
            };
            foreach (var test in tests) {
                PrintParticularValue(test.input, test.output);
                PrintParticularValue("+" + test.input, test.output);
                PrintParticularValue("-" + test.input, "-" + test.output);
            }
        }

        [Test]
        public void ThreeThirds() {
            string program = @"void main() {
    print_float((1.0 / 3.0) * 3.0);
}";
            var results = Run(program);
            Assert.AreEqual("1.000000", results.ProgramOutput);
        }

        [Test]
        public void TwoThirds() {
            string program = @"void main() {
    print_float((1.0 / 3.0) * 2.0);
}";
            var results = Run(program);
            Assert.AreEqual("0.666667", results.ProgramOutput);
        }

        [Test]
        public void OneThird() {
            string program = @"void main() {
    print_float((1.0 / 3.0) * 1.0);
}";
            var results = Run(program);
            Assert.AreEqual("0.333333", results.ProgramOutput);
        }
    }
}
