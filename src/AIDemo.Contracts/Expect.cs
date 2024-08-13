using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Serious;

public static class Expect
{
    /// <summary>
    /// A helper that throws <see cref="UnreachableException"/> if the object is <c>null</c>.
    /// Use this over <c>!.</c> when you *know* the object is non-<c>null</c>.
    /// If the object _is_ <c>null</c>, we throw a well-known exception and have alerts monitoring that exception.
    /// After calling this method, the compiler will assume whatever object was passed in is, in fact, not <c>null</c>.
    /// </summary>
    /// <remarks>
    /// Not to be confused with <see cref="ArgumentNullException.ThrowIfNull"/> which is specifically used for arguments to a method.
    /// This method is used when accessing a non-argument such as a computed value or a value returned from a method.
    /// </remarks>
    /// <param name="o">The object that should not be <c>null</c></param>
    /// <param name="errorMessage">An optional error message to use in the exception thrown if the object is <c>null</c></param>
    /// <param name="expression">The expression passed in via CallerArgumentExpression</param>
    /// <returns>The original object provided in <paramref name="o"/></returns>
    public static T NotNull<T>([NotNull] T? o, string? errorMessage = null, [CallerArgumentExpression("o")] string? expression = null)
        where T : class
    {
        if (o is null)
        {
            if (errorMessage is not { Length: > 0 })
            {
                errorMessage = expression is { Length: > 0 } ? $"The expression '{expression}' should not be null." : "Expected a non-null value.";
            }

            throw new UnreachableException(errorMessage);
        }

        return o;
    }

    /// <summary>
    /// A helper that throws <see cref="UnreachableException"/> if the value is <c>null</c>.
    /// Use this over <c>!.</c> when you *know* the value is non-<c>null</c>.
    /// If the value _is_ <c>null</c>, we throw a well-known exception and have alerts monitoring that exception.
    /// After calling this method, the compiler will assume whatever value was passed in is, in fact, not <c>null</c>.
    /// </summary>
    /// <remarks>
    /// Not to be confused with <see cref="ArgumentNullException.ThrowIfNull"/> which is specifically used for arguments to a method.
    /// This method is used when accessing a non-argument such as a computed value or a value returned from a method.
    /// </remarks>
    /// <param name="v">The value that should not be <c>null</c></param>
    /// <param name="errorMessage">An optional error message to use in the exception thrown if the value is <c>null</c></param>
    /// <param name="expression">The expression passed in via CallerArgumentExpression</param>
    /// <returns>The original value provided in <paramref name="v"/></returns>
    public static T NotNull<T>([NotNull] T? v, string? errorMessage = null, [CallerArgumentExpression("v")] string? expression = null)
        where T : struct
    {
        if (v is null)
        {
            if (errorMessage is not { Length: > 0 })
            {
                errorMessage = expression is { Length: > 0 } ? $"The expression '{expression}' should not be null." : "Expected a non-null value.";
            }

            throw new UnreachableException(errorMessage);
        }

        return v.Value;
    }

    /// <summary>
    /// A helper that throws <see cref="UnreachableException"/> if the value is not <c>true</c>.
    /// If the value is <c>false</c>, we throw a well-known exception and have alerts monitoring that exception.
    /// </summary>
    /// <param name="v">The value that should be <c>true</c></param>
    /// <param name="errorMessage">An optional error message to use in the exception thrown if the value is <c>false</c></param>
    /// <param name="expression">The expression passed in via CallerArgumentExpression</param>
    public static void True([DoesNotReturnIf(false)] bool v, string? errorMessage = null, [CallerArgumentExpression("v")] string? expression = null)
    {
        if (v is not true)
        {
            if (errorMessage is not { Length: > 0 })
            {
                errorMessage = expression is { Length: > 0 } ? $"The expression '{expression}' must be true." : $"Expected true but got false.";
            }

            throw new UnreachableException(errorMessage);
        }
    }

    /// <summary>
    /// A helper that throws <see cref="UnreachableException"/> if the value is <c>null</c> or if the value is not a
    /// Type <typeparamref name="T"/>.
    /// Use this over <c>!.</c> when you *know* the value is non-<c>null</c>.
    /// If the value _is_ <c>null</c>, we throw a well-known exception and have alerts monitoring that exception.
    /// After calling this method, the compiler will assume whatever value was passed in is, in fact, not <c>null</c>.
    /// </summary>
    /// <remarks>
    /// Not to be confused with <see cref="ArgumentNullException.ThrowIfNull"/> which is specifically used for arguments to a method.
    /// This method is used when accessing a non-argument such as a computed value or a value returned from a method.
    /// </remarks>
    /// <param name="v">The value that should not be <c>null</c></param>
    /// <param name="errorMessage">An optional error message to use in the exception thrown if the value is <c>null</c></param>
    /// <param name="expression">The expression passed in via CallerArgumentExpression</param>
    /// <returns>The original value provided in <paramref name="v"/></returns>
    public static T Type<T>([NotNull] object? v, string? errorMessage = null, [CallerArgumentExpression("v")] string? expression = null)
        where T : class
    {
        Expect.NotNull(v, errorMessage, expression);
        if (v is not T result)
        {
            if (errorMessage is not { Length: > 0 })
            {
                errorMessage = expression is { Length: > 0 } ? $"The expression '{expression}' must be a {typeof(T)}, but got a {v.GetType()}.." : $"Expected a non-null value of type {typeof(T)}, but got a {v.GetType()}.";
            }

            throw new UnreachableException(errorMessage);
        }

        return result;
    }
}

public static class ExpectExtensions
{
    // Ok, I'm usually pretty uncomfortable with extension methods on common types like object or open generics, but this might be worth it.
    // Similarly, I don't really like extension methods that accept 'null' for their 'this' parameter, but again, I think value outweighs cost here.
    //  - anurse
    /// <summary>
    /// Asserts that the object is not <c>null</c>, and returns it.
    /// Because this is an extension method, it is "safe" to "call" it on a <c>null</c> value.
    /// It will throw the expected exception when given a <c>null</c> object, but it is otherwise safe to call.
    /// </summary>
    /// <param name="o">The object to check for <c>null</c></param>
    /// <param name="expression">Provided by the compiler, contains the expression that was used to provide the value for <paramref name="o"/></param>
    /// <param name="errorMessage">An optional error message to use in the exception thrown if the value is <c>null</c></param>
    /// <returns>The original object provided in <paramref name="o"/></returns>
    public static T Require<T>([NotNull] this T? o, string? errorMessage = null, [CallerArgumentExpression("o")] string? expression = null)
        where T : class
    {
        return Expect.NotNull(o, errorMessage, expression);
    }

    /// <summary>
    /// Asserts that the object is not <c>null</c> and of type <typeparamref name="T"/>, and returns it.
    /// Because this is an extension method, it is "safe" to "call" it on a <c>null</c> value.
    /// It will throw the expected exception when given a <c>null</c> object, but it is otherwise safe to call.
    /// </summary>
    /// <param name="o">The object to check for <c>null</c></param>
    /// <param name="expression">Provided by the compiler, contains the expression that was used to provide the value for <paramref name="o"/></param>
    /// <param name="errorMessage">An optional error message to use in the exception thrown if the value is <c>null</c></param>
    /// <returns>The original object provided in <paramref name="o"/></returns>
    public static T Require<T>([NotNull] this object? o, string? errorMessage = null, [CallerArgumentExpression("o")] string? expression = null)
        where T : class
    {
        return Expect.Type<T>(o, errorMessage, expression);
    }

    /// <summary>
    /// Asserts that the <see cref="System.Nullable"/> is not <c>null</c>, and returns the interior value.
    /// Because this is an extension method, it is "safe" to "call" it on a <c>null</c> value.
    /// It will throw the expected exception when given a <c>null</c> value, but it is otherwise safe to call.
    /// </summary>
    /// <param name="v">The value to check for <c>null</c></param>
    /// <param name="expression">Provided by the compiler, contains the expression that was used to provide the value for <paramref name="v"/></param>
    /// <param name="errorMessage">An optional error message to use in the exception thrown if the value is <c>null</c></param>
    /// <returns>The original value provided in <paramref name="v"/></returns>
    public static T Require<T>([NotNull] this T? v, string? errorMessage = null, [CallerArgumentExpression("v")] string? expression = null)
        where T : struct
    {
        return Expect.NotNull(v, errorMessage, expression);
    }

    /// <summary>
    /// Asserts that the <see cref="Task"/> result is not <c>null</c>, and returns it.
    /// It will throw the expected exception when resolving to a <c>null</c> value, but it is otherwise safe to call.
    /// </summary>
    /// <param name="task">The <see cref="Task"/> whose result to check for <c>null</c></param>
    /// <param name="expression">Provided by the compiler, contains the expression that was used to provide the value for <paramref name="task"/></param>
    /// <param name="errorMessage">An optional error message to use in the exception thrown if the value is <c>null</c></param>
    /// <returns>The original object provided in <paramref name="task"/></returns>
    public static async Task<T> Require<T>(this Task<T?> task, string? errorMessage = null, [CallerArgumentExpression("task")] string? expression = null)
        where T : class
    {
        return Expect.NotNull(await task, errorMessage, expression);
    }

    /// <summary>
    /// Asserts that the <see cref="ValueTask"/> result is not <c>null</c>, and returns it.
    /// It will throw the expected exception when resolving to a <c>null</c> value, but it is otherwise safe to call.
    /// </summary>
    /// <param name="task">The <see cref="ValueTask"/> whose result to check for <c>null</c></param>
    /// <param name="expression">Provided by the compiler, contains the expression that was used to provide the value for <paramref name="task"/></param>
    /// <param name="errorMessage">An optional error message to use in the exception thrown if the value is <c>null</c></param>
    /// <returns>The original object provided in <paramref name="task"/></returns>
    public static async ValueTask<T> Require<T>(this ValueTask<T?> task, string? errorMessage = null, [CallerArgumentExpression("task")] string? expression = null)
        where T : class
    {
        return Expect.NotNull(await task, errorMessage, expression);
    }
}

