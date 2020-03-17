using NUnit.Framework;

namespace Tests.Lang
{
    public class HelloWorld : ExampleLangTest
    {
        public HelloWorld() : base(@"
int main() {

}
") {
        }

        [Test]
        public void SymbolTableTest() {
            SymbolTableExists("::global::main")
                .WithNoRows();
            SymbolTableExists("::global")
                .WithRow((NAME_COLUMN, "main"), (ENTITY_TYPE_COLUMN, ENTITY_TYPE_FUNCTION))
                .WithColumn(TYPE_COLUMN, "int");
        }
    }
}
