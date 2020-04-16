using NUnit.Framework;

namespace Tests.ExampleLang
{
    public class ForLoopTests : LangUnitTestSuite
    {
        [Test]
        public void ForLoop_Variant_Full() {
            string program = @"void main() {
    for (int i = 0; i < 10; i = i + 1) {
        print_int(i);
    }
}";
            var results = Run(program);
            Assert.AreEqual("0123456789", results.ProgramOutput);
        }

        [Test]
        public void ForLoop_Variant_NoInit() {
            string program = @"void main() {
    int i = 0;
    for (; i < 10; i = i + 1) {
        print_int(i);
    }
}";
            var results = Run(program);
            Assert.AreEqual("0123456789", results.ProgramOutput);
        }

        [Test]
        public void ForLoop_Variant_NoInit_NoCompare() {
            string program = @"void main() {
    int i = 0;
    for (; ; i = i + 1) {
        if (i >= 10) {
            return;
        }
        print_int(i);
    }
}";
            var results = Run(program);
            Assert.AreEqual("0123456789", results.ProgramOutput);
        }

        [Test]
        public void ForLoop_Variant_NoInit_NoCompare_NoUpdate() {
            string program = @"void main() {
    int i = 0;
    for (;;) {
        if (i >= 10) {
            return;
        }
        print_int(i);
        i = i + 1;
    }
}";
            var results = Run(program);
            Assert.AreEqual("0123456789", results.ProgramOutput);
        }
    }
}
