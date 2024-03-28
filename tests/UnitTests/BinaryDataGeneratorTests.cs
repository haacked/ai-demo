using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Serious.ChatFunctions;

namespace UnitTests;

public class BinaryDataGeneratorTests
{
    public class TheGetParametersDictionaryMethod
    {
        [Fact]
        public void WithSimpleArgumentsReturnsExpectedDictionary()
        {
            // Arrange
            var expected = new Dictionary<string, object>
            {
                { "type", "object" },
                { "properties", new Dictionary<string, object>
                    {
                        { "name", new Dictionary<string, object>
                        {
                            { "type", "string" },
                            { "description", "The name of the person." }
                        } },
                        { "Age", new Dictionary<string, object> { { "type", "integer" } } },
                        { "FavoriteColor", new Dictionary<string, object>
                            {
                                { "type", "string" },
                                { "enum", new[] { "Red", "Green", "Blue" } }
                            }
                        }
                    }
                },
                { "required", new[] { "name", "Age" } }
            };

            // Act
            var actual = BinaryDataGenerator.GetParametersDictionary(typeof(SimpleArguments));

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WithComplexArgumentsReturnsExpectedDictionary()
        {
            // Arrange
            var expected = new Dictionary<string, object>
            {
                { "type", "object" },
                { "properties", new Dictionary<string, object>
                    {
                        { "args", new Dictionary<string, object>
                            {
                                { "type", "object" },
                                { "description", "The arguments."},
                                { "properties", new Dictionary<string, object>
                                    {
                                        { "name", new Dictionary<string, object>
                                        {
                                            { "type", "string" },
                                            { "description", "The name of the person." }
                                        } },
                                        { "Age", new Dictionary<string, object> { { "type", "integer" } } },
                                        { "FavoriteColor", new Dictionary<string, object>
                                            {
                                                { "type", "string" },
                                                { "enum", new[] { "Red", "Green", "Blue" } }
                                            }
                                        }
                                    }
                                },
                                { "required", new[] { "name", "Age" } }
                            }
                        },
                        { "Justification", new Dictionary<string, object> { { "type", "string" } } }
                    }
                },
                { "required", new[] { "args" } }
            };

            // Act
            var actual = BinaryDataGenerator.GetParametersDictionary(typeof(ComplexArguments));

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WithArrayArgumentReturnsExpectedDictionary()
        {
            // Arrange
            var expected = new Dictionary<string, object>
            {
                { "type", "object" },
                { "properties", new Dictionary<string, object>
                    {
                        { "names", new Dictionary<string, object>
                            {
                                { "type", "array" },
                                { "description", "The names of the people." },
                                { "items", new Dictionary<string, object>
                                    {
                                        { "type", "string" }
                                    }
                                }
                            }
                        },
                        { "Ages", new Dictionary<string, object>
                            {
                                { "type", "array" },
                                { "items", new Dictionary<string, object>
                                    {
                                        { "type", "integer" }
                                    }
                                }
                            }
                        },
                    }
                },
                { "required", new[] { "names" } }
            };

            // Act
            var actual = BinaryDataGenerator.GetParametersDictionary(typeof(ArrayArguments));

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WithComplexArrayArgumentReturnsExpectedDictionary()
        {
            // Arrange
            var expected = new Dictionary<string, object>
            {
                { "type", "object" },
                { "properties", new Dictionary<string, object>
                    {
                        { "args", new Dictionary<string, object>
                            {
                                { "type", "array" },
                                { "description", "The args." },
                                { "items", new Dictionary<string, object>
                                    {
                                        { "type", "object" },
                                        { "properties", new Dictionary<string, object>
                                            {
                                                { "name", new Dictionary<string, object>
                                                {
                                                    { "type", "string" },
                                                    { "description", "The name of the person." }
                                                } },
                                                { "Age", new Dictionary<string, object> { { "type", "integer" } } },
                                                { "FavoriteColor", new Dictionary<string, object>
                                                    {
                                                        { "type", "string" },
                                                        { "enum", new[] { "Red", "Green", "Blue" } }
                                                    }
                                                }
                                            }
                                        },
                                        { "required", new[] { "name", "Age" } }
                                    }
                                }
                            }
                        }
                    }
                },
                { "required", new[] { "args" } }
            };

            // Act
            var actual = BinaryDataGenerator.GetParametersDictionary(typeof(ComplexArrayArguments));

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DoesNotStackOverflow()
        {
            // Arrange
            var expected = new Dictionary<string, object>
            {
                { "type", "object" },
                { "properties", new Dictionary<string, object>
                    {
                        { "Args", new Dictionary<string, object>
                            {
                                { "type", "array" },
                                { "items", new Dictionary<string, object>() }
                            }
                        }
                    }
                },
                { "required", Array.Empty<object>() }
            };

            // Act
            var actual = BinaryDataGenerator.GetParametersDictionary(typeof(StackOverflowArguments));

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}

public record SimpleArguments(
    [property: Required]
    [property: Description("The name of the person.")]
    [property: JsonPropertyName("name")]
    string Name,

    [property: Required]
    int Age,

    Colors? FavoriteColor);

public record ComplexArguments(
    [property: Required]
    [property: Description("The arguments.")]
    [property: JsonPropertyName("args")]
    SimpleArguments Arguments,

    string? Justification);

public enum Colors
{
    Red,
    Green,
    Blue
}

public record ArrayArguments(
    [property: Required]
    [property: Description("The names of the people.")]
    [property: JsonPropertyName("names")]
    IReadOnlyList<string> Names,

    IReadOnlyList<int> Ages);

public record ComplexArrayArguments(
    [property: Required]
    [property: Description("The args.")]
    [property: JsonPropertyName("args")]
    IReadOnlyList<SimpleArguments> Args);

public record StackOverflowArguments(IReadOnlyList<StackOverflowArguments> Args);