using NUnit.Framework;

namespace Vtex.SemanticVersioning.Tests
{
    [TestFixture]
    class VersionIdentifierTests
    {
        private class CompareTo
        {
            [TestCase("x", "x")]
            [TestCase("abc", "abc")]
            [TestCase("", "")]
            [TestCase(123, 123)]
            public void Equal(object a, object b)
            {
                // Arrange
                var first = Convert(a);
                var second = Convert(b);

                // Act & Assert
                Assert.That(first.CompareTo(second), Is.EqualTo(0));
            }

            [TestCase(2, 1)]
            [TestCase("abc", 123)]
            [TestCase("b", "a")]
            public void Positive_result(object a, object b)
            {
                // Arrange
                var first = Convert(a);
                var second = Convert(b);

                // Act & Assert
                Assert.That(first.CompareTo(second), Is.EqualTo(1));
            }

            [TestCase(1, 2)]
            [TestCase(123, "abc")]
            [TestCase("a", "b")]
            public void Negative_result(object a, object b)
            {
                // Arrange
                var first = Convert(a);
                var second = Convert(b);

                // Act & Assert
                Assert.That(first.CompareTo(second), Is.EqualTo(-1));
            }
        }

        public static VersionIdentifier Convert(object value)
        {
            return value is string
                ? new VersionIdentifier(value as string)
                : new VersionIdentifier((int) value);
        }
    }
}
