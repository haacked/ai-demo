using AIDemoWeb.Library;

namespace UnitTests;

public class ReflectionExtensionsTests
{
    public class TheGetCollectionTypeMethod
    {
        [Theory]
        [InlineData(typeof(IEnumerable<string>), typeof(string))]
        [InlineData(typeof(IReadOnlyList<string>), typeof(string))]
        [InlineData(typeof(List<string>), typeof(string))]
        [InlineData(typeof(string[]), typeof(string))]
        [InlineData(typeof(object), null)]
        public void WithListTypeReturnsExpectedType(Type type, Type? expected)
        {
            var actual = type.GetCollectionElementType();

            Assert.Equal(expected, actual);
        }
    }
}