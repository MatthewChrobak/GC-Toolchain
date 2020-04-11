using Core;
using NUnit.Framework;

namespace Tests.ExampleLang
{
    public class FunctionTests : LangUnitTestSuite
    {
        [Test]
        public void Program_HasMain_Allowed() {
            string program = @"void main() { }";
            var results = new ExampleLangTest(program);
        }

        [Test]
        public void Program_NoMain_NotAllowed() {
            string program = @"int notMain() { }";
            AssertExceptionCause(program, "Program must have a main.");
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
            AssertExceptionCause(program, "The function main at (1, 20) already exists.");
        }

        [Test]
        public void FunctionParameters_CorrectNumber_CorrectTypes_Allowed() {
            string program = @"void Test(int x, int y) { } int main() { Test(1, 2); }";
            var results = new ExampleLangTest(program);

            results.SymbolTableExists("::global::Test").WithExactlyNRows(2, (Column.IsParameter, true));
            results.SymbolTableExists("::global::Test").WithOneRow((Column.ParameterIndex, 0)).WithColumn(Column.Name, "x");
            results.SymbolTableExists("::global::Test").WithOneRow((Column.ParameterIndex, 1)).WithColumn(Column.Name, "y");
        }

        [Test]
        public void FunctionParameters_IncorrectNumber_CorrectTypes_NotAllowed() {
            string program = @"void Test(int x, int y, int z) { } int main() { Test(1, 2); }";
            AssertExceptionCause(program, "Expected 3 parameters at (1, 49). Got 2.");
        }

        [Test]
        public void FunctionParameters_IncorrectNumber_IncorrectTypes_NotAllowed() {
            string program = @"void Test(float x, float y, float z) { } int main() { Test(1, 2); }";
            AssertExceptionCause(program, "Expected 3 parameters at (1, 55). Got 2.");
        }

        [Test]
        public void FunctionParameters_CorrectNumber_IncorrectTypes_NotAllowed() {
            string program = @"void Test(float x, float y) { } int main() { Test(1, 2); }";
            AssertExceptionCause(program, "Expected (float, float). Got (int, int).");
        }

        [Test]
        public void FunctionArguments_CorrectNumber_CorrectTypes_Allowed() {
            string program = @"void Test(int x, int y) { } int main() { Test(1, 2); }";
            var results = new ExampleLangTest(program);

            results.SymbolTableExists("::global::Test").WithExactlyNRows(2, (Column.IsParameter, true));
            results.SymbolTableExists("::global::Test").WithOneRow((Column.ParameterIndex, 0)).WithColumn(Column.Name, "x");
            results.SymbolTableExists("::global::Test").WithOneRow((Column.ParameterIndex, 1)).WithColumn(Column.Name, "y");
        }

        [Test]
        public void FunctionArguments_IncorrectNumber_CorrectTypes_NotAllowed() {
            string program = @"void Test(int x, int y) { } int main() { Test(1, 2, 3); }";
            AssertExceptionCause(program, "Expected 2 parameters at (1, 42). Got 3.");
        }

        [Test]
        public void FunctionArguments_IncorrectNumber_IncorrectTypes_NotAllowed() {
            string program = @"void Test(float x, float y) { } int main() { Test(1, 2, 3); }";
            AssertExceptionCause(program, "Expected 2 parameters at (1, 46). Got 3.");
        }

        [Test]
        public void FunctionArguments_CorrectNumber_IncorrectTypes_NotAllowed() {
            string program = @"void Test(float x, float y) { } int main() { Test(1, 2); }";
            AssertExceptionCause(program, "Expected (float, float). Got (int, int).");
        }

        [Test]
        public void FunctionReturnType_Void_Allowed() {
            string program = @"void main() { }";
            var results = new ExampleLangTest(program);
            results.SymbolTableExists("::global")
                .WithOneRow((Column.EntityType, "function"), (Column.Name, "main"))
                .WithColumn(Column.ReturnType, "void");
        }

        [Test]
        public void FunctionReturnType_InvalidType_NotAllowed() {
            string program = @"abc main() { }";
            AssertExceptionCause(program, "Unknown return type 'abc' at (1, 1).");
        }
    }
}
