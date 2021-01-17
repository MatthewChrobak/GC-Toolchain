using Core;
using Core.Config;
using NUnit.Framework;

namespace Tests.Core
{
    public class RuleConfigurationFileTests
    {
        [Test]
        public void DuplicateRuleValues() {
            string config = @"
#rule One
A
#rule Two
A
";
            Assert.Throws<AssertionFailedException>(new TestDelegate(() => {
                new RuleConfigurationFile(config.Split("\r\n"), "config", null);
            }));
        }
    }
}
