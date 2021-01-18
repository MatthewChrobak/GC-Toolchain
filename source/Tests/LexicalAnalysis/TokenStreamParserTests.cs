using Core;
using Core.LexicalAnalysis;
using Core.Logging;
using GCTDefault.LexicalAnalysis;
using NUnit.Framework;
using System;

namespace Tests.LexicalAnalysis
{
    public class TokenStreamParserTests
    {
        private TokenParser ConstructParser(string configuration) {
            var log = new Log();
            var config = new LexicalConfigurationFile(configuration.Split("\r\n"), "config", log);
            var nfa = new TokenParserTableGenerator(config, log).NFATable;
            return new TokenParser(nfa, log);
        }

        private void VerifyNewline(TokenStream tokens) {
            Assert.AreEqual(tokens.Next.TokenType, "whitespace");
            Assert.AreEqual(tokens.Next.TokenType, "whitespace");
        }

        private TokenStream GetTokenStreamFromConfig(string configuration, string program) {
            var parser = ConstructParser(configuration);
            return parser.Parse(program);
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

#token whitespace
%0D
%0A
";
            string program = @":";
            var tokens = GetTokenStreamFromConfig(config, program);
            Assert.AreEqual(tokens.Next.TokenType, ":");
            Assert.AreEqual(tokens.Current.Content, ":");
            VerifyNewline(tokens);
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

#token whitespace
%0D
%0A
";
            string program = @":1";
            var tokens = GetTokenStreamFromConfig(config, program);
            Assert.AreEqual(tokens.Next.TokenType, ":1");
            Assert.AreEqual(tokens.Current.Content, ":1");
            VerifyNewline(tokens);
        }

        [Test]
        public void Range_Hex_Val() {
            string config = @"
#rule range_inclusive
-

#token range2
%43-D

#token whitespace
%0D
%0A
";
            string program = @"CD";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(tokens.Next.TokenType, "range2");
            Assert.AreEqual(tokens.Next.TokenType, "range2");
            VerifyNewline(tokens);
        }

        [Test]
        public void Range_Val_Hex() {
            string config = @"
#rule range_inclusive
-

#token range3
E-%46

#token whitespace
%0D
%0A
";
            string program = @"EF";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(tokens.Next.TokenType, "range3");
            Assert.AreEqual(tokens.Next.TokenType, "range3");
            VerifyNewline(tokens);
        }

        [Test]
        public void Range_Val_Val() {
            string config = @"
#rule range_inclusive
-

#token range4
G-H

#token whitespace
%0D
%0A
";
            string program = @"GH";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(tokens.Next.TokenType, "range4");
            Assert.AreEqual(tokens.Next.TokenType, "range4");
            VerifyNewline(tokens);
        }

        [Test]
        public void Range_Hex_Hex() {
            string config = @"
#rule range_inclusive
-

#token range1
%41-%42

#token whitespace
%0D
%0A
";
            string program = @"AB";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(tokens.Next.TokenType, "range1");
            Assert.AreEqual(tokens.Next.TokenType, "range1");
            VerifyNewline(tokens);
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
            Assert.AreEqual(program.Replace("\r\n", " ").Length + 1, count);
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

#token whitespace
%20
%0D
%0A
";
            string program = "abcd";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.IsTrue(tokens.HasNext);
            Assert.AreEqual(3, tokens.Count);
            var token = tokens.Next;

            Assert.AreEqual("high_priority", token.TokenType);
            Assert.AreEqual(program, token.Content);
            Assert.AreEqual(1, token.Row);
            VerifyNewline(tokens);
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

#token whitespace
A0D
A0A
";
            string program = ((char)byteVal).ToString();
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(tokens.Count, 3); // Account for return carriage and newline
            Assert.IsTrue(tokens.HasNext);
            var token = tokens.Next;
            Assert.AreEqual("hex", token.TokenType);
            Assert.AreEqual(1, token.Row);
            Assert.AreEqual(1, token.Column);
            Assert.AreEqual(((char)byteVal).ToString(), token.Content);
            VerifyNewline(tokens);
            Assert.IsFalse(tokens.HasNext);
        }

        [Test]
        public void Range() {
            string config = @$"
#rule {LexicalConfigurationFile.RULE_RANGE_INCLUSIVE_KEY}
A

#token range
aAz

#token whitespace
%0D
%0A";
            string program = "abcdefghijklmnopqrstuvwxyz";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(tokens.Count, program.Length + 2);
            int i = 1;
            while (tokens.HasNext) {
                var token = tokens.Next;
                Assert.AreEqual(token.TokenType, "range");
                Assert.AreEqual(1, token.Row);
                Assert.AreEqual(i, token.Column);
                Assert.AreEqual(program[i - 1].ToString(), token.Content);
                i++;

                if (i > program.Length) {
                    break;
                }
            }
            VerifyNewline(tokens);
            Assert.IsFalse(tokens.HasNext);
        }

        [Test]
        public void ZeroOrMore_SubTokens() {
            string config = $@"
#rule {LexicalConfigurationFile.RULE_ZERO_OR_MORE_KEY}
F

#subtoken zero_or_more_subtoken
a

#token zero_or_more
$zero_or_more_subtokenF

#token whitespace
%0D
%0A
";
            string program = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(3, tokens.Count);
            var token = tokens.Next;
            Assert.AreEqual(1, token.Row);
            Assert.AreEqual(1, token.Column);
            Assert.AreEqual("zero_or_more", token.TokenType);
            Assert.AreEqual(program, token.Content);
            VerifyNewline(tokens);
            Assert.IsFalse(tokens.HasNext);
        }

        [Test]
        public void ZeroOrMore_SubToken() {
            string config = $@"
#rule {LexicalConfigurationFile.RULE_ZERO_OR_MORE_KEY}
F

#subtoken zero_or_more_subtoken
aF

#token zero_or_more
$zero_or_more_subtoken

#token whitespace
%0D
%0A
";
            string program = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(3, tokens.Count);
            var token = tokens.Next;
            Assert.AreEqual(1, token.Row);
            Assert.AreEqual(1, token.Column);
            Assert.AreEqual("zero_or_more", token.TokenType);
            Assert.AreEqual(program, token.Content);
            VerifyNewline(tokens);
            Assert.IsFalse(tokens.HasNext);
        }

        [Test]
        public void ZeroOrMore() {
            string config = $@"
#rule {LexicalConfigurationFile.RULE_ZERO_OR_MORE_KEY}
F

#token zero_or_more
aF

#token whitespace
%0D
%0A
";
            string program = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.AreEqual(3, tokens.Count);
            var token = tokens.Next;
            Assert.AreEqual(1, token.Row);
            Assert.AreEqual(1, token.Column);
            Assert.AreEqual("zero_or_more", token.TokenType);
            Assert.AreEqual(program, token.Content);
            VerifyNewline(tokens);
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

#token whitespace
%20
%0D
%0A
";
            string invalid_program = "a";
            var parser = ConstructParser(config);

            Assert.Throws<AssertionFailedException>(new TestDelegate(() => {
                parser.Parse(invalid_program);
            }));

            string valid_program = "aaz";
            var tokens = GetTokenStreamFromConfig(config, valid_program);

            Assert.IsTrue(tokens.HasNext);
            Assert.AreEqual(3, tokens.Count); // Account for the newline
            var token = tokens.Next;
            Assert.AreEqual("range", token.TokenType);
            Assert.AreEqual(1, token.Column);
            Assert.AreEqual(1, token.Row);
            Assert.AreEqual(valid_program, token.Content);
            VerifyNewline(tokens);
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

#token whitespace
%0D
%0A
";
            string program = "abcd";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.IsTrue(tokens.HasNext);
            Assert.AreEqual(tokens.Count, 3);
            var token = tokens.Next;
            Assert.AreEqual("final_token", token.TokenType);
            Assert.AreEqual(1, token.Column);
            Assert.AreEqual(1, token.Row);
            Assert.AreEqual(program, token.Content);
            VerifyNewline(tokens);
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
%0D
%0A
";
            string program = "0 123 0123";
            var tokens = GetTokenStreamFromConfig(config, program);

            Assert.IsTrue(tokens.HasNext);
            var token = tokens.Next;
            Assert.AreEqual("integer", token.TokenType);
            Assert.AreEqual(1, token.Row);
            Assert.AreEqual(1, token.Column);
            Assert.AreEqual("0", token.Content);

            Assert.IsTrue(tokens.HasNext);
            token = tokens.Next;
            Assert.AreEqual("whitespace", token.TokenType);
            Assert.AreEqual(1, token.Row);
            Assert.AreEqual(2, token.Column);
            Assert.AreEqual(" ", token.Content);

            Assert.IsTrue(tokens.HasNext);
            token = tokens.Next;
            Assert.AreEqual("integer", token.TokenType);
            Assert.AreEqual(1, token.Row);
            Assert.AreEqual(3, token.Column);
            Assert.AreEqual("123", token.Content);

            Assert.IsTrue(tokens.HasNext);
            token = tokens.Next;
            Assert.AreEqual("whitespace", token.TokenType);
            Assert.AreEqual(1, token.Row);
            Assert.AreEqual(6, token.Column);
            Assert.AreEqual(token.Content, " ");

            Assert.IsTrue(tokens.HasNext);
            token = tokens.Next;
            Assert.AreEqual("integer", token.TokenType);
            Assert.AreEqual(1, token.Row);
            Assert.AreEqual(7, token.Column);
            Assert.AreEqual("0", token.Content);

            Assert.IsTrue(tokens.HasNext);
            token = tokens.Next;
            Assert.AreEqual("integer", token.TokenType);
            Assert.AreEqual(1, token.Row);
            Assert.AreEqual(8, token.Column);
            Assert.AreEqual("123", token.Content);
        }
    }
}
