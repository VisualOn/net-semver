using NUnit.Framework;

namespace Vtex.SemanticVersioning.Tests
{
    class ComparatorTests
    {
        [TestCase("", "1.2.3")]
        [TestCase(">1.2.3", "1.2.4")]
        [TestCase(">=1.2.3", "1.2.3")]
        [TestCase(">=1.2.3", "1.2.4")]
        [TestCase("<1.2.3", "1.2.2")]
        [TestCase("<=1.2.3", "1.2.3")]
        [TestCase("<=1.2.3", "1.2.2")]
        [TestCase("<=1.2.3", "1.2.3-beta")]
        public void Should_test_ok(string comp, string version)
        {
            // Arrange
            var comparator = Comparator.Parse(comp);

            // Act
            var result = comparator.Matches(Version.Parse(version));

            // Assert
            Assert.That(result, Is.True);
        }

        [TestCase(">1.2.3", "1.2.3")]
        [TestCase(">1.2.3", "1.2.2")]
        [TestCase(">=1.2.3", "1.2.2")]
        [TestCase("<1.2.3", "1.2.3")]
        [TestCase("<1.2.3", "1.2.4")]
        [TestCase("<=1.2.3", "1.2.4")]
        [TestCase("<1.2.3", "1.2.3-beta")]
        public void Should_fail_test(string comp, string version)
        {
            // Arrange
            var comparator = Comparator.Parse(comp);

            // Act
            var result = comparator.Matches(Version.Parse(version));

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
