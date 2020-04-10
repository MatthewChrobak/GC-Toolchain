using Core;
using NUnit.Framework;

namespace Tests.ExampleLang
{
    public class LangUnitTestSuite
    {
        protected void AssertExceptionCause(AssertionFailedException e, string expected) {
            string actual = e.Message;
            if (actual.Length - expected.Length < 0) {
                Assert.Fail($"length of '{actual}' is less than length of '{expected}'");
            }

            string end = actual[^expected.Length..];
            Assert.AreEqual(expected, end);
        }
    }
}
