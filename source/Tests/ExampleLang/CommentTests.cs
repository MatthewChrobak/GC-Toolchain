using NUnit.Framework;

namespace Tests.ExampleLang
{
    public class CommentTests : LangUnitTestSuite
    {
        [Test]
        public void SingleLineComment_OwnLine_Ignored() {
            string program = @"void main() {
    // This comment is ignored.
    print_int(10);
}";
            var results = Run(program);
            Assert.AreEqual("10", results.ProgramOutput);
        }

        [Test]
        public void MultiLineComment_OwnLines_Ignored() {
            string program = @"void main() {
    /* 
        This comment is ignored.
    */
    print_int(10);
}";
            var results = Run(program);
            Assert.AreEqual("10", results.ProgramOutput);
        }

        [Test]
        public void SingleLineComment_ExistingLine_Ignored() {
            string program = @"void main() { // This comment is ignored.
    print_int(10);
}";
            var results = Run(program);
            Assert.AreEqual("10", results.ProgramOutput);
        }

        [Test]
        public void MultiLineComment_ExistingLines_Ignored() {
            string program = @"void main() { /* 
        This comment is ignored.
    */ print_int(10);
}";
            var results = Run(program);
            Assert.AreEqual("10", results.ProgramOutput);
        }
    }
}
