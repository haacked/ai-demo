namespace Haack.AIDemoWeb.Library;

public static class MathExtensions
{
    /// <summary>
    /// Calculates the Cosine similarity between two vectors.
    /// </summary>
    /// <remarks>
    /// Cosine similarity measures the similarity between two vectors of an inner product space.
    /// It is measured by the cosine of the angle between two vectors and determines whether two
    /// vectors are pointing in roughly the same direction. It is often used to measure document
    /// similarity in text analysis.
    /// </remarks>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static float CosineSimilarity(
        this IReadOnlyCollection<float> vector1,
        IReadOnlyCollection<float> vector2)
    {
        if (vector1.Count != vector2.Count)
            throw new ArgumentException("Vectors must have the same length.");

        // Calculate dot product
        var dotProduct = vector1.Zip(vector2, (a, b) => a * b).Sum();

        // Calculate magnitude of vector1
        var magnitude1 = (float)Math.Sqrt(vector1.Sum(x => x * x));

        // Calculate magnitude of vector2
        var magnitude2 = (float)Math.Sqrt(vector2.Sum(x => x * x));

        // Calculate cosine similarity
        return dotProduct / (magnitude1 * magnitude2);
    }

    public static long DoArithmetic(ArithmeticArguments arguments)
    {
        var result = arguments switch
        {
            { ArithmeticOperation: ArithmeticOperation.Add } => arguments.Left + arguments.Right,
            { ArithmeticOperation: ArithmeticOperation.Subtract } => arguments.Left - arguments.Right,
            { ArithmeticOperation: ArithmeticOperation.Multiply } => arguments.Left * arguments.Right,
            { ArithmeticOperation: ArithmeticOperation.Divide } => arguments.Left / arguments.Right,
            _ => throw new InvalidOperationException("Unknown operation.")
        };
        return result;
    }
}

public record ArithmeticArguments(long Left, ArithmeticOperation ArithmeticOperation, long Right, long? Answer = null);

public enum ArithmeticOperation
{
    Add,
    Subtract,
    Multiply,
    Divide,
}
