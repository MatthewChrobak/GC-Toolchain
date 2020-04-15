using Core;
using NUnit.Framework;

namespace Tests.ExampleLang
{
    public class LangUnitTestSuite
    {
        protected void AssertExceptionCause(string program, string expected) {
            var e = Assert.Catch<AssertionFailedException>(() => Run(program));
            string actual = e.Message;
            if (actual.Length - expected.Length < 0) {
                Assert.Fail($"length of '{actual}' is less than length of '{expected}'");
            }

            string end = actual[^expected.Length..];
            Assert.AreEqual(expected, end);
        }

        protected ExampleLangTest Run(string program) {
            return new ExampleLangTest(program);
        }
    }
}
