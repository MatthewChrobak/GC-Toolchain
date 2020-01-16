using LexicalAnalysis;
using NUnit.Framework;

namespace Tests.LexicalAnalysis
{
    public class TokenStreamParserTests
    {
        [Test]
        public void Test() {
            string _config_string = @"
#subtoken nonzero
1-9

#subtoken digit
0-9

#token integer
$nonzero $digit*
0

#token whitespace
%20
";
            string _program_string = "0 123 0123";
            var config = new LexicalConfigurationFile(_config_string.Split("\r\n"), "config");
            var nfa = new TokenParserTableGenerator(config).NFATable;
            var parser = new TokenParser(nfa);
            var tokens = parser.ParseString(_program_string).GetEnumerator();

            Assert.IsTrue(tokens.MoveNext());
            Assert.AreEqual(tokens.Current.TokenType, "integer");
            Assert.AreEqual(tokens.Current.Row, 0);
            Assert.AreEqual(tokens.Current.Column, 0);
            Assert.AreEqual(tokens.Current.Content, "0");

            Assert.IsTrue(tokens.MoveNext());
            Assert.AreEqual(tokens.Current.TokenType, "whitespace");
            Assert.AreEqual(tokens.Current.Row, 0);
            Assert.AreEqual(tokens.Current.Column, 1);
            Assert.AreEqual(tokens.Current.Content, " ");

            Assert.IsTrue(tokens.MoveNext());
            Assert.AreEqual(tokens.Current.TokenType, "integer");
            Assert.AreEqual(tokens.Current.Row, 0);
            Assert.AreEqual(tokens.Current.Column, 2);
            Assert.AreEqual(tokens.Current.Content, "123");

            Assert.IsTrue(tokens.MoveNext());
            Assert.AreEqual(tokens.Current.TokenType, "whitespace");
            Assert.AreEqual(tokens.Current.Row, 0);
            Assert.AreEqual(tokens.Current.Column, 5);
            Assert.AreEqual(tokens.Current.Content, " ");

            Assert.IsTrue(tokens.MoveNext());
            Assert.AreEqual(tokens.Current.TokenType, "integer");
            Assert.AreEqual(tokens.Current.Row, 0);
            Assert.AreEqual(tokens.Current.Column, 6);
            Assert.AreEqual(tokens.Current.Content, "0");

            Assert.IsTrue(tokens.MoveNext());
            Assert.AreEqual(tokens.Current.TokenType, "integer");
            Assert.AreEqual(tokens.Current.Row, 0);
            Assert.AreEqual(tokens.Current.Column, 7);
            Assert.AreEqual(tokens.Current.Content, "123");
        }
    }
}
