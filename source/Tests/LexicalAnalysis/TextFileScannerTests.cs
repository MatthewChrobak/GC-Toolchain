using LexicalAnalysis;
using NUnit.Framework;
using System.IO;

namespace Tests.LexicalAnalysis
{
    public class TextFileScannerTests
    {
        [Test]
        public void ReadFileContent() {
            string fileContent = "12345";
            string path = $"{nameof(ReadFileContent)}.txt";
            File.WriteAllText(path, fileContent);

            using (var fr = new TextFileScanner(path)) {

                for (int i = 0; i < fileContent.Length; i++) {
                    char c = fr.Read();
                    Assert.AreEqual(c, fileContent[i]);
                }
                // Two extra reads for the ending /r/n
                fr.Read();
                fr.Read();
                Assert.IsFalse(fr.CanRead);
            }
            File.Delete(path);
        }

        [Test]
        public void GoNext() {
            string fileContent = "12345";
            string path = $"{nameof(GoNext)}.txt";
            File.WriteAllText(path, fileContent);

            using (var fr = new TextFileScanner(path)) {
                // + 2 for \r\n
                for (int i = 0; i < fileContent.Length + 2; i++) {
                    fr.GoNext();
                }

                Assert.IsFalse(fr.CanRead);
            }
            File.Delete(path);
        }

        [Test]
        public void GoPrevious() {
            string fileContent = "12345";
            string path = $"{nameof(GoPrevious)}.txt";
            File.WriteAllText(path, fileContent);

            using (var fr = new TextFileScanner(path)) {
                fr.GoToEnd();
                // + 2 for \r\n
                for (int i = 0; i <= fileContent.Length + 2 - 1; i++) {
                    fr.GoPrevious();
                }
                Assert.AreEqual(fr.Line, 0);
                Assert.AreEqual(fr.Column, 0);
                Assert.IsTrue(fr.CanRead);
            }
            File.Delete(path);
        }

        [Test]
        public void GoToEnd() {
            string fileContent = "12345";
            string path = $"{nameof(GoToEnd)}.txt";
            File.WriteAllText(path, fileContent);

            using (var fr = new TextFileScanner(path)) {
                fr.GoToEnd();

                Assert.AreEqual(fr.Line, 0);
                Assert.AreEqual(fr.Peek(), TextFileScanner.EOF);
                // + 2 to account for \r\n
                Assert.AreEqual(fr.Column, fileContent.Length + 2);
                Assert.IsFalse(fr.CanRead);
            }
            File.Delete(path);
        }

        [Test]
        public void GoToStart() {
            string fileContent = "12345";
            string path = $"{nameof(GoToStart)}.txt";
            File.WriteAllText(path, fileContent);

            using (var fr = new TextFileScanner(path)) {
                while (fr.CanRead) {
                    fr.Read();
                }

                fr.GoToStart();
                Assert.IsTrue(fr.CanRead);
            }
            File.Delete(path);
        }

        [Test]
        public void ReadMultiLine() {
            string fileContent = @"1
2
3
4";
            string path = $"{nameof(ReadMultiLine)}.txt";
            File.WriteAllText(path, fileContent);

            int count = 0;
            using (var fr = new TextFileScanner(path)) {
                while (fr.CanRead) {
                    fr.Read();
                    count++;
                }
            }
            File.Delete(path);
            // + 2 to account for the last \r\n
            Assert.AreEqual(fileContent.Length + 2, count);
        }
    }
}
