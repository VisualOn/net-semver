using NUnit.Framework;

namespace Vtex.SemanticVersioning.Tests
{
    [TestFixture]
    class SemVerTests
    {
        [TestCase("1.0.0 - 2.0.0", "1.2.3")]
        [TestCase("1.0.0", "1.0.0")]
        [TestCase(">=*", "0.2.4")]
        [TestCase("", "1.0.0")]
        [TestCase("*", "1.2.3")]
        [TestCase(">=1.0.0", "1.0.0")]
        [TestCase(">=1.0.0", "1.0.1")]
        [TestCase(">=1.0.0", "1.1.0")]
        [TestCase(">1.0.0", "1.0.1")]
        [TestCase(">1.0.0", "1.1.0")]
        [TestCase("<=2.0.0", "2.0.0")]
        [TestCase("<=2.0.0", "1.9999.9999")]
        [TestCase("<=2.0.0", "0.2.9")]
        [TestCase("<2.0.0", "1.9999.9999")]
        [TestCase("<2.0.0", "0.2.9")]
        [TestCase(">= 1.0.0", "1.0.0")]
        [TestCase(">=  1.0.0", "1.0.1")]
        [TestCase(">=   1.0.0", "1.1.0")]
        [TestCase("> 1.0.0", "1.0.1")]
        [TestCase(">  1.0.0", "1.1.0")]
        [TestCase("<=   2.0.0", "2.0.0")]
        [TestCase("<= 2.0.0", "1.9999.9999")]
        [TestCase("<=  2.0.0", "0.2.9")]
        [TestCase("<    2.0.0", "1.9999.9999")]
        [TestCase("<\t2.0.0", "0.2.9")]
        [TestCase(">=0.1.97", "0.1.97")]
        [TestCase("0.1.20 || 1.2.4", "1.2.4")]
        [TestCase(">=0.2.3 || <0.0.1", "0.0.0")]
        [TestCase(">=0.2.3 || <0.0.1", "0.2.3")]
        [TestCase(">=0.2.3 || <0.0.1", "0.2.4")]
        [TestCase("||", "1.3.4")]
        [TestCase("2.x.x", "2.1.3")]
        [TestCase("1.2.x", "1.2.3")]
        [TestCase("1.2.x || 2.x", "2.1.3")]
        [TestCase("1.2.x || 2.x", "1.2.3")]
        [TestCase("x", "1.2.3")]
        [TestCase("2.*.*", "2.1.3")]
        [TestCase("1.2.*", "1.2.3")]
        [TestCase("1.2.* || 2.*", "2.1.3")]
        [TestCase("1.2.* || 2.*", "1.2.3")]
        [TestCase("*", "1.2.3")]
        [TestCase("2", "2.1.2")]
        [TestCase("2.3", "2.3.1")]
        [TestCase("~2.4", "2.4.0")] // >=2.4.0 <2.5.0
        [TestCase("~2.4", "2.4.5")]
        [TestCase("~>3.2.1", "3.2.2")] // >=3.2.1 <3.3.0,
        [TestCase("~1", "1.2.3")] // >=1.0.0 <2.0.0
        [TestCase("~>1", "1.2.3")]
        [TestCase("~> 1", "1.2.3")]
        [TestCase("~1.0", "1.0.2")] // >=1.0.0 <1.1.0,
        [TestCase("~ 1.0", "1.0.2")]
        [TestCase("~ 1.0.3", "1.0.12")]
        [TestCase(">=1", "1.0.0")]
        [TestCase(">= 1", "1.0.0")]
        [TestCase("<1.2", "1.1.1")]
        [TestCase("< 1.2", "1.1.1")]
        [TestCase("~v0.5.4-pre", "0.5.5")]
        [TestCase("~v0.5.4-pre", "0.5.4")]
        [TestCase("=0.7.x", "0.7.2")]
        [TestCase(">=0.7.x", "0.7.2")]
        [TestCase("=0.7.x", "0.7.0-asdf")]
        [TestCase(">=0.7.x", "0.7.0-asdf")]
        [TestCase("<=0.7.x", "0.6.2")]
        [TestCase("~1.2.1 >=1.2.3", "1.2.3")]
        [TestCase("~1.2.1 =1.2.3", "1.2.3")]
        [TestCase("~1.2.1 1.2.3", "1.2.3")]
        [TestCase("~1.2.1 >=1.2.3 1.2.3", "1.2.3")]
        [TestCase("~1.2.1 1.2.3 >=1.2.3", "1.2.3")]
        [TestCase("~1.2.1 1.2.3", "1.2.3")]
        [TestCase(">=1.2.1 1.2.3", "1.2.3")]
        [TestCase("1.2.3 >=1.2.1", "1.2.3")]
        [TestCase(">=1.2.3 >=1.2.1", "1.2.3")]
        [TestCase(">=1.2.1 >=1.2.3", "1.2.3")]
        [TestCase("<=1.2.3", "1.2.3-beta")]
        [TestCase(">1.2", "1.3.0-beta")]
        [TestCase(">=1.2", "1.2.8")]
        [TestCase("^1.2.3", "1.8.1")]
        [TestCase("^1.2.3", "1.2.3-beta")]
        [TestCase("^0.1.2", "0.1.2")]
        [TestCase("^0.1", "0.1.2")]
        [TestCase("^1.2", "1.4.2")]
        [TestCase("^1.2 ^1", "1.4.2")]
        [TestCase("^1.2", "1.2.0-pre")]
        [TestCase("^1.2.3", "1.2.3-pre")]
        [TestCase(">1", "2.0.0")]
        public void Satisfies_strict(string range, string source)
        {
            // Arrange
            var version = Version.Parse(source);

            // Act
            var satisfies = version.Satisfies(range);

            // Assert
            Assert.That(satisfies, Is.True);
        }

        [TestCase("*", "v1.2.3-foo")]
        [TestCase(">=0.1.97", "v0.1.97")]
        [TestCase("1", "1.0.0beta")]
        public void Satisfies_loose(string range, string source)
        {
            // Arrange
            var version = Version.Parse(source, true);

            // Act
            var satisfies = version.Satisfies(range);

            // Assert
            Assert.That(satisfies, Is.True);
        }

        [TestCase("1.0.0 - 2.0.0", "2.2.3")]
        [TestCase("1.0.0", "1.0.1")]
        [TestCase(">=1.0.0", "0.0.0")]
        [TestCase(">=1.0.0", "0.0.1")]
        [TestCase(">=1.0.0", "0.1.0")]
        [TestCase(">1.0.0", "0.0.1")]
        [TestCase(">1.0.0", "0.1.0")]
        [TestCase("<=2.0.0", "3.0.0")]
        [TestCase("<=2.0.0", "2.9999.9999")]
        [TestCase("<=2.0.0", "2.2.9")]
        [TestCase("<2.0.0", "2.9999.9999")]
        [TestCase("<2.0.0", "2.2.9")]
        [TestCase(">=0.1.97", "0.1.93")]
        [TestCase("0.1.20 || 1.2.4", "1.2.3")]
        [TestCase(">=0.2.3 || <0.0.1", "0.0.3")]
        [TestCase(">=0.2.3 || <0.0.1", "0.2.2")]
        [TestCase("2.x.x", "1.1.3")]
        [TestCase("2.x.x", "3.1.3")]
        [TestCase("1.2.x", "1.3.3")]
        [TestCase("1.2.x || 2.x", "3.1.3")]
        [TestCase("1.2.x || 2.x", "1.1.3")]
        [TestCase("2.*.*", "1.1.3")]
        [TestCase("2.*.*", "3.1.3")]
        [TestCase("1.2.*", "1.3.3")]
        [TestCase("1.2.* || 2.*", "3.1.3")]
        [TestCase("1.2.* || 2.*", "1.1.3")]
        [TestCase("2", "1.1.2")]
        [TestCase("2.3", "2.4.1")]
        [TestCase("~2.4", "2.5.0")] // >=2.4.0 <2.5.0
        [TestCase("~2.4", "2.3.9")]
        [TestCase("~>3.2.1", "3.3.2")] // >=3.2.1 <3.3.0
        [TestCase("~>3.2.1", "3.2.0")] // >=3.2.1 <3.3.0
        [TestCase("~1", "0.2.3")] // >=1.0.0 <2.0.0
        [TestCase("~>1", "2.2.3")]
        [TestCase("~1.0", "1.1.0")] // >=1.0.0 <1.1.0
        [TestCase("<1", "1.0.0")]
        [TestCase(">=1.2", "1.1.1")]
        [TestCase("~v0.5.4-beta", "0.5.4-alpha")]
        [TestCase("=0.7.x", "0.8.2")]
        [TestCase(">=0.7.x", "0.6.2")]
        [TestCase("<=0.7.x", "0.7.2")]
        [TestCase("<1.2.3", "1.2.3-beta")]
        [TestCase("=1.2.3", "1.2.3-beta")]
        [TestCase(">1.2", "1.2.8")]
        [TestCase("^1.2.3", "2.0.0-alpha")]
        [TestCase("^1.2.3", "1.2.2")]
        [TestCase("^1.2", "1.1.9")]
        // invalid ranges never satisfied!
        [TestCase("blerg", "1.2.3")]
        [TestCase("^1.2.3", "2.0.0-pre")]
        public void Does_not_satisfy_strict(string range, string source)
        {
            // Arrange
            var version = Version.Parse(source);

            // Act
            var satisfies = version.Satisfies(range);

            // Assert
            Assert.That(satisfies, Is.False);
        }

        [TestCase(">=0.1.97", "v0.1.93")]
        [TestCase("1", "2.0.0beta")]
        [TestCase("<1", "1.0.0beta")]
        [TestCase("< 1", "1.0.0beta")]
        [TestCase("git+https://user:password0123@github.com/foo", "123.0.0")]
        public void Does_not_satisfy_loose(string range, string source)
        {
            // Arrange
            var version = Version.Parse(source, true);

            // Act
            var satisfies = version.Satisfies(range);

            // Assert
            Assert.That(satisfies, Is.False);
        }
    }
}
