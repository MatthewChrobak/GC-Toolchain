using Core.LexicalAnalysis;
using NUnit.Framework;

namespace Tests.LexicalAnalysis
{
    public class TextFileScannerTests
    {
        [Test]
        public void ReadFileContent() {
            string content = "12345";

            using (var fr = new TextFileScanner(content)) {

                for (int i = 0; i < content.Length; i++) {
                    char c = fr.Read();
                    Assert.AreEqual(c, content[i]);
                }
                // Two extra reads for the ending /r/n
                fr.Read();
                fr.Read();
                Assert.IsFalse(fr.CanRead);
            }
        }

        [Test]
        public void GoNext() {
            string content = "12345";

            using (var fr = new TextFileScanner(content)) {
                // + 2 for \r\n
                for (int i = 0; i < content.Length + 2; i++) {
                    fr.GoNext();
                }

                Assert.IsFalse(fr.CanRead);
            }
        }

        [Test]
        public void GoPrevious() {
            string content = "12345";

            using (var fr = new TextFileScanner(content)) {
                fr.GoToEnd();
                // + 2 for \r\n
                for (int i = 0; i <= content.Length + 2 - 1; i++) {
                    fr.GoPrevious();
                }
                Assert.AreEqual(fr.Line, 0);
                Assert.AreEqual(fr.Column, 0);
                Assert.IsTrue(fr.CanRead);
            }
        }

        [Test]
        public void GoToEnd() {
            string content = "12345";

            using (var fr = new TextFileScanner(content)) {
                fr.GoToEnd();

                Assert.AreEqual(fr.Line, 0);
                Assert.AreEqual(fr.Peek(), TextFileScanner.EOF);
                // + 2 to account for \r\n
                Assert.AreEqual(fr.Column, content.Length + 2);
                Assert.IsFalse(fr.CanRead);
            }
        }

        [Test]
        public void GoToStart() {
            string content = "12345";

            using (var fr = new TextFileScanner(content)) {
                while (fr.CanRead) {
                    fr.Read();
                }

                fr.GoToStart();
                Assert.IsTrue(fr.CanRead);
            }
        }

        [Test]
        public void ReadMultiLine() {
            string content = @"1
2
3
4";

            int count = 0;
            using (var fr = new TextFileScanner(content)) {
                while (fr.CanRead) {
                    fr.Read();
                    count++;
                }
            }
            // + 2 to account for the last \r\n
            Assert.AreEqual(content.Length + 2, count);
        }
    }
}
