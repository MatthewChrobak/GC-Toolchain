using Core;
using NUnit.Framework;

namespace Tests.ExampleLang
{
    public class SymbolTableTests : LangUnitTestSuite
    {
        [Test]
        public void Program_HasMain_Allowed() {
            string program = @"void main() { }";
            var results = new ExampleLangTest(program);
        }

        [Test]
        public void Program_NoMain_NotAllowed() {
            string program = @"int notMain() { }";
            var e = Assert.Catch<AssertionFailedException>(() => new ExampleLangTest(program));
            AssertExceptionCause(e, "Program must have a main.");
        }

        [Test]
        public void FunctionDefinition_Multiple_Allowed() {
            string program = @"void main() { } int main2() { }";
            var results = new ExampleLangTest(program);

            results.SymbolTableExists("::global::main");
            results.SymbolTableExists("::global::main2");
        }

        [Test]
        public void FunctionDefinition_Single_Allowed() {
            string program = @"void main() { }";
            var results = new ExampleLangTest(program);

            results.SymbolTableExists("::global::main");
        }

        [Test]
        public void FunctionDefinition_DuplicateName_NotAllowed() {
            string program = @"int main() { } int main() { }";
            var e = Assert.Catch<AssertionFailedException>(() => new ExampleLangTest(program));
            AssertExceptionCause(e, "The function main at (1, 20) already exists.");
        }

        [Test]
        public void VariableDefinition_NoAssignment_Allowed() {
            string program = @"void main() {
    int x;
}";
            var results = new ExampleLangTest(program);

            results.SymbolTableExists("::global::main").WithOneRow(("entity_type", "variable"), ("name", "x"), ("type", "int"));
        }

        [Test]
        public void VariableDefinition_Assignment_Allowed() {
            string program = @"void main() {
    int x = 1;
}";
            var results = new ExampleLangTest(program);

            results.SymbolTableExists("::global::main").WithOneRow(("entity_type", "variable"), ("name", "x"), ("type", "int"));
        }

        [Test]
        public void VariableDeclaration_Redifinition_NotAllowed() {
            string program = @"int main() {
    int x;
    int x;
}";
            var e = Assert.Catch<AssertionFailedException>(() => new ExampleLangTest(program));
            AssertExceptionCause(e, "The variable 'x' at (1, 35) already exists in ::global::main.");
        }

        [Test]
        public void FunctionParameters_CorrectNumber_CorrectTypes_Allowed() {
            string program = @"void Test(int x, int y) { } int main() { Test(1, 2); }";
            var results = new ExampleLangTest(program);

            results.SymbolTableExists("::global::Test").WithExactlyNRows(2, ("is_parameter", true));
            results.SymbolTableExists("::global::Test").WithOneRow(("parameter_index", 0)).WithColumn("name", "x");
            results.SymbolTableExists("::global::Test").WithOneRow(("parameter_index", 1)).WithColumn("name", "y");
        }

        [Test]
        public void FunctionParameters_IncorrectNumber_CorrectTypes_NotAllowed() {
            string program = @"void Test(int x, int y, int z) { } int main() { Test(1, 2); }";
            var e = Assert.Catch<AssertionFailedException>(() => new ExampleLangTest(program));
            AssertExceptionCause(e, "Expected 3 parameters at (1, 49). Got 2.");
        }

        [Test]
        public void FunctionParameters_IncorrectNumber_IncorrectTypes_NotAllowed() {
            string program = @"void Test(float x, float y, float z) { } int main() { Test(1, 2); }";
            var e = Assert.Catch<AssertionFailedException>(() => new ExampleLangTest(program));
            AssertExceptionCause(e, "Expected 3 parameters at (1, 55). Got 2.");
        }

        [Test]
        public void FunctionParameters_CorrectNumber_IncorrectTypes_NotAllowed() {
            string program = @"void Test(float x, float y) { } int main() { Test(1, 2); }";
            var e = Assert.Catch<AssertionFailedException>(() => new ExampleLangTest(program));
            AssertExceptionCause(e, "Expected (float, float). Got (int, int).");
        }

        [Test]
        public void FunctionArguments_CorrectNumber_CorrectTypes_Allowed() {
            string program = @"void Test(int x, int y) { } int main() { Test(1, 2); }";
            var results = new ExampleLangTest(program);

            results.SymbolTableExists("::global::Test").WithExactlyNRows(2, ("is_parameter", true));
            results.SymbolTableExists("::global::Test").WithOneRow(("parameter_index", 0)).WithColumn("name", "x");
            results.SymbolTableExists("::global::Test").WithOneRow(("parameter_index", 1)).WithColumn("name", "y");
        }

        [Test]
        public void FunctionArguments_IncorrectNumber_CorrectTypes_NotAllowed() {
            string program = @"void Test(int x, int y) { } int main() { Test(1, 2, 3); }";
            var e = Assert.Catch<AssertionFailedException>(() => new ExampleLangTest(program));
            AssertExceptionCause(e, "Expected 2 parameters at (1, 42). Got 3.");
        }

        [Test]
        public void FunctionArguments_IncorrectNumber_IncorrectTypes_NotAllowed() {
            string program = @"void Test(float x, float y) { } int main() { Test(1, 2, 3); }";
            var e = Assert.Catch<AssertionFailedException>(() => new ExampleLangTest(program));
            AssertExceptionCause(e, "Expected 2 parameters at (1, 46). Got 3.");
        }

        [Test]
        public void FunctionArguments_CorrectNumber_IncorrectTypes_NotAllowed() {
            string program = @"void Test(float x, float y) { } int main() { Test(1, 2); }";
            var e = Assert.Catch<AssertionFailedException>(() => new ExampleLangTest(program));
            AssertExceptionCause(e, "Expected (float, float). Got (int, int).");
        }
    }
}
