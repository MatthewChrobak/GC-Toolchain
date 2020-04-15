using Core;
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
            var results = Run(program);
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
            var results = Run(program);
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
            var results = Run(program);
            Assert.AreEqual("2010", results.ProgramOutput);
        }

        [Test]
        public void VariableDeclaration_KnownType() {
            string program = @"void main() { int x; }";
            var results = Run(program);

            results.SymbolTableExists("::global::main").WithOneRow((Column.EntityType, "variable"), (Column.Name, "x")).WithColumn(Column.Type, "int");
        }

        [Test]
        public void VariableDeclaration_UnknownType() {
            string program = @"void main() { abc x; }";
            AssertExceptionCause(program, "Unknown variable type 'abc' at (1, 15).");
        }

        [Test]
        public void VariableDeclaration_VoidType_NotAllowed() {
            string program = @"void main() { void x; }";
            AssertExceptionCause(program, "Unknown variable type 'void' at (1, 15).");
        }

        [Test]
        public void VariableDefinition_NoAssignment_Allowed() {
            string program = @"void main() {
    int x;
}";
            var results = Run(program);

            results.SymbolTableExists("::global::main").WithOneRow((Column.EntityType, "variable"), (Column.Name, "x"), (Column.Type, "int"));
        }

        [Test]
        public void VariableDefinition_Assignment_Allowed() {
            string program = @"void main() {
    int x = 1;
}";
            var results = Run(program);

            results.SymbolTableExists("::global::main").WithOneRow((Column.EntityType, "variable"), (Column.Name, "x"), (Column.Type, "int"));
        }

        [Test]
        public void VariableDeclaration_Redifinition_NotAllowed() {
            string program = @"int main() {
    int x;
    int x;
}";
            AssertExceptionCause(program, "The variable 'x' at (1, 35) already exists in ::global::main.");
        }
    }
}
