using Core;
using LexicalAnalysis;
using NUnit.Framework;
using System;

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
        public void RangeLiteral_NoRHS() {
            string config = @"
#rule range_inclusive
:

#rule literal_prefix
\

#token :
\:
";
            string program = @":";
            var tokens = GetTokenStreamFromConfig(config, program);
            Assert.AreEqual(tokens.Next.TokenType, ":");
            Assert.AreEqual(tokens.Current.Content, ":");
        }

        [Test]
        public void RangeLiteral_RHS() {
            string config = @"
#rule range_inclusive
:

#rule literal_prefix
\

#token :1
\:1
";
            string program = @":1";
            var tokens = GetTokenStreamFromConfig(config, program);
            Assert.AreEqual(tokens.Next.TokenType, ":1");
            Assert.AreEqual(tokens.Current.Content, ":1");
        }

        [Test]
        public void Range_Hex_Val() {
            string config = @"
#rule range_inclusive
-

#token range2
%43-D
";
            string program = @"CD";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(tokens.Next.TokenType, "range2");
            Assert.AreEqual(tokens.Next.TokenType, "range2");
        }

        [Test]
        public void Range_Val_Hex() {
            string config = @"
#rule range_inclusive
-

#token range3
E-%46
";
            string program = @"EF";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(tokens.Next.TokenType, "range3");
            Assert.AreEqual(tokens.Next.TokenType, "range3");
        }

        [Test]
        public void Range_Val_Val() {
            string config = @"
#rule range_inclusive
-

#token range4
G-H
";
            string program = @"GH";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(tokens.Next.TokenType, "range4");
            Assert.AreEqual(tokens.Next.TokenType, "range4");
        }

        [Test]
        public void Range_Hex_Hex() {
            string config = @"
#rule range_inclusive
-

#token range1
%41-%42
";
            string program = @"AB";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(tokens.Next.TokenType, "range1");
            Assert.AreEqual(tokens.Next.TokenType, "range1");
        }

        [Test]
        public void ReadMultiLines() {
            string config = @"
#token number
1

#token newline
%D %A
";
            string program = @"1
1
1
1";
            var tokens = GetTokenStreamFromConfig(config, program);

            int count = 0;
            while (tokens.HasNext) {
                var token = tokens.Next;
                count++;
            }
            Assert.AreEqual(program.Replace("\r\n", " ").Length, count);
        }

        [Test]
        public void TokenWithNoHeader() {
            string config = @"
#token
$subtoken
";
            Assert.Throws<AssertionFailedException>(new TestDelegate(() => {
                ConstructParser(config);
            }));
        }

        [Test]
        public void TokenWithNoBody() {
            string config = @"
#token test
";
            Assert.Throws<AssertionFailedException>(new TestDelegate(() => {
                ConstructParser(config);
            }));
        }

        [Test]
        public void SubTokenWithNoBody() {
            string config = @"
#subtoken test
";
            Assert.Throws<AssertionFailedException>(new TestDelegate(() => {
                ConstructParser(config);
            }));
        }

        [Test]
        public void UnknownSubtoken() {
            string config = @"
#token Test
$subtoken
";
            Assert.Throws<AssertionFailedException>(new TestDelegate(() => {
                ConstructParser(config);
            }));
        }

        [Test]
        public void PriorityMatching_EqualPriority() {
            string config = @$"
#token low_priority {LexicalConfigurationFile.HEADER_PRIORITY_PREFIX}0
abcd

#token high_priority {LexicalConfigurationFile.HEADER_PRIORITY_PREFIX}0
abcd
";
            string program = "abcd";
            Assert.Throws<AssertionFailedException>(new TestDelegate(() => {
                var tokens = GetTokenStreamFromConfig(config, program);
            }));
        }

        [Test]
        public void PriorityMatching_TakeHigh() {
            string config = @$"
#token low_priority {LexicalConfigurationFile.HEADER_PRIORITY_PREFIX}0
abcd

#token high_priority {LexicalConfigurationFile.HEADER_PRIORITY_PREFIX}1
abcd
";
            string program = "abcd";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.IsTrue(tokens.HasNext);
            Assert.AreEqual(1, tokens.Count);
            var token = tokens.Next;

            Assert.AreEqual("high_priority", token.TokenType);
            Assert.AreEqual(program, token.Content);
            Assert.AreEqual(0, token.Row);
            Assert.IsFalse(tokens.HasNext);
        }

        [Test]
        public void Hex() {
            // + 1 because '\0' would not work.
            byte byteVal = (byte)((new Random().Next() % 255) + 1);
            string val = BitConverter.ToString(new byte[] { byteVal });
            string config = $@"
#rule {LexicalConfigurationFile.RULE_HEX_PREFIX_KEY}
A

#token hex
A{val}
";
            string program = ((char)byteVal).ToString();
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(tokens.Count, 1);
            Assert.IsTrue(tokens.HasNext);
            var token = tokens.Next;
            Assert.AreEqual("hex", token.TokenType);
            Assert.AreEqual(0, token.Row);
            Assert.AreEqual(0, token.Column);
            Assert.AreEqual(((char)byteVal).ToString(), token.Content);
            Assert.IsFalse(tokens.HasNext);
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
            Assert.IsFalse(tokens.HasNext);
        }

        [Test]
        public void ZeroOrMore_SubTokens() {
            string config = $@"
#rule {LexicalConfigurationFile.RULE_ZERO_OR_MORE_KEY}
A

#subtoken zero_or_more_subtoken
a

#token zero_or_more
$zero_or_more_subtokenA
";
            string program = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(1, tokens.Count);
            var token = tokens.Next;
            Assert.AreEqual(0, token.Row);
            Assert.AreEqual(0, token.Column);
            Assert.AreEqual("zero_or_more", token.TokenType);
            Assert.AreEqual(program, token.Content);
            Assert.IsFalse(tokens.HasNext);
        }

        [Test]
        public void ZeroOrMore_SubToken() {
            string config = $@"
#rule {LexicalConfigurationFile.RULE_ZERO_OR_MORE_KEY}
A

#subtoken zero_or_more_subtoken
aA

#token zero_or_more
$zero_or_more_subtoken
";
            string program = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(1, tokens.Count);
            var token = tokens.Next;
            Assert.AreEqual(0, token.Row);
            Assert.AreEqual(0, token.Column);
            Assert.AreEqual("zero_or_more", token.TokenType);
            Assert.AreEqual(program, token.Content);
            Assert.IsFalse(tokens.HasNext);
        }

        [Test]
        public void ZeroOrMore() {
            string config = $@"
#rule {LexicalConfigurationFile.RULE_ZERO_OR_MORE_KEY}
A

#token zero_or_more
aA
";
            string program = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(1, tokens.Count);
            var token = tokens.Next;
            Assert.AreEqual(0, token.Row);
            Assert.AreEqual(0, token.Column);
            Assert.AreEqual("zero_or_more", token.TokenType);
            Assert.AreEqual(program, token.Content);
            Assert.IsFalse(tokens.HasNext);
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
