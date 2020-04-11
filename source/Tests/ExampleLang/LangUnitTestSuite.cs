using Core;
using NUnit.Framework;

namespace Tests.ExampleLang
{
    public class LangUnitTestSuite
    {
        protected void AssertExceptionCause(string program, string expected) {
            var e = Assert.Catch<AssertionFailedException>(() => new ExampleLangTest(program));
            string actual = e.Message;
            if (actual.Length - expected.Length < 0) {
                Assert.Fail($"length of '{actual}' is less than length of '{expected}'");
            }

            string end = actual[^expected.Length..];
            Assert.AreEqual(expected, end);
        }
    }
}
