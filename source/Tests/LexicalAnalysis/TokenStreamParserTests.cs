using Core;
using LexicalAnalysis;
using NUnit.Framework;

namespace Tests.LexicalAnalysis
{
    public class TokenStreamParserTests
    {
        private TokenParser ConstructParser(string configuration) {
            var config = new LexicalConfigurationFile(configuration.Split("\r\n"), "config");
            var nfa = new TokenParserTableGenerator(config).NFATable;
            return new TokenParser(nfa);
        }

        private TokenStream GetTokenStreamFromConfig(string configuration, string program) {
            var parser = ConstructParser(configuration);
            return parser.ParseString(program);
        }

        [Test]
        public void Range() {
            string config = @$"
#rule {LexicalConfigurationFile.RULE_RANGE_INCLUSIVE_KEY}
A

#token range
aAz
";
            string program = "abcdefghijklmnopqrstuvwxyz";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(tokens.Count, program.Length);
            int i = 0;
            while (tokens.HasNext) {
                var token = tokens.Next;
                Assert.AreEqual(token.TokenType, "range");
                Assert.AreEqual(token.Row, 0);
                Assert.AreEqual(token.Column, i);
                Assert.AreEqual(token.Content, program[i].ToString());
                i++;
            }
        }

        [Test]
        public void Literal_Range() {
            string config = $@"
#rule {LexicalConfigurationFile.RULE_RANGE_INCLUSIVE_KEY}
a
#rule {LexicalConfigurationFile.RULE_LITERAL_PREFIX_KEY}
B

#token range
Baaz
";
            string invalid_program = "a";
            var parser = ConstructParser(config);

            Assert.Throws<AssertionFailedException>(new TestDelegate(() => {
                parser.ParseString(invalid_program);
            }));

            string valid_program = "aaz";
            var tokens = GetTokenStreamFromConfig(config, valid_program);

            Assert.IsTrue(tokens.HasNext);
            Assert.AreEqual(tokens.Count, 1);
            var token = tokens.Next;
            Assert.AreEqual("range", token.TokenType);
            Assert.AreEqual(token.Column, 0);
            Assert.AreEqual(token.Row, 0);
            Assert.AreEqual(token.Content, valid_program);
            Assert.IsFalse(tokens.HasNext);
        }

        [Test]
        public void SubTokenPrefix_Literal() {
            string config = $@"
#rule {LexicalConfigurationFile.RULE_SUB_TOKEN_PREFIX_KEY}
a

#rule {LexicalConfigurationFile.RULE_LITERAL_PREFIX_KEY}
z

#subtoken sub_token
zabcd

#token final_token
asub_token

";
            string program = "abcd";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.IsTrue(tokens.HasNext);
            Assert.AreEqual(tokens.Count, 1);
            var token = tokens.Next;
            Assert.AreEqual("final_token", token.TokenType);
            Assert.AreEqual(token.Column, 0);
            Assert.AreEqual(token.Row, 0);
            Assert.AreEqual(token.Content, program);
            Assert.IsFalse(tokens.HasNext);
        }

        [Test]
        public void BasicParse() {
            string config = @"
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
            string program = "0 123 0123";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.IsTrue(tokens.HasNext);
            var token = tokens.Next;
            Assert.AreEqual("integer", token.TokenType);
            Assert.AreEqual(token.Row, 0);
            Assert.AreEqual(token.Column, 0);
            Assert.AreEqual(token.Content, "0");

            Assert.IsTrue(tokens.HasNext);
            token = tokens.Next;
            Assert.AreEqual("whitespace", token.TokenType);
            Assert.AreEqual(token.Row, 0);
            Assert.AreEqual(token.Column, 1);
            Assert.AreEqual(token.Content, " ");

            Assert.IsTrue(tokens.HasNext);
            token = tokens.Next;
            Assert.AreEqual("integer", token.TokenType);
            Assert.AreEqual(token.Row, 0);
            Assert.AreEqual(token.Column, 2);
            Assert.AreEqual(token.Content, "123");

            Assert.IsTrue(tokens.HasNext);
            token = tokens.Next;
            Assert.AreEqual("whitespace", token.TokenType);
            Assert.AreEqual(token.Row, 0);
            Assert.AreEqual(token.Column, 5);
            Assert.AreEqual(token.Content, " ");

            Assert.IsTrue(tokens.HasNext);
            token = tokens.Next;
            Assert.AreEqual("integer", token.TokenType);
            Assert.AreEqual(token.Row, 0);
            Assert.AreEqual(token.Column, 6);
            Assert.AreEqual(token.Content, "0");

            Assert.IsTrue(tokens.HasNext);
            token = tokens.Next;
            Assert.AreEqual("integer", token.TokenType);
            Assert.AreEqual(token.Row, 0);
            Assert.AreEqual(token.Column, 7);
            Assert.AreEqual(token.Content, "123");
        }
    }
}
