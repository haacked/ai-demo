using System.Net.Http.Headers;

namespace AIDemo.Library;

public static class HttpContextExtensions
{
    /// <summary>
    /// Reads <paramref name="httpContent"/> as a string without throwing.
    /// </summary>
    /// <param name="httpContent">The <see cref="HttpContent"/>.</param>
    /// <param name="headers">The request or response <see cref="HttpHeaders"/>.</param>
    /// <param name="logger">An optional <see cref="ILogger"/> for exceptions thrown while reading.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The text content, or <see langword="null"/>.</returns>
    public static async Task<string?> SafelyReadAsStringAsync(
        this HttpContent? httpContent,
        HttpHeaders headers,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return httpContent switch
            {
                StringContent content =>
                    await content.ReadAsStringAsync(cancellationToken),
                { } content
                    when headers.IsTextBasedContentType()
                        || httpContent.Headers.IsTextBasedContentType() =>
                    await content.ReadAsStringAsync(cancellationToken),
                _ => null,
            };
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            logger?.ErrorReadingAsString(ex);
            return null;
        }
    }

    static readonly string[] TextContentTypes = { "html", "text", "xml", "json", "txt", "x-www-form-urlencoded" };

    static bool IsTextBasedContentType(this HttpHeaders headers)
    {
        if (!headers.TryGetValues("Content-Type", out var values))
            return false;

        return values.Any(
            header => TextContentTypes.Any(
                t => header.Contains(t, StringComparison.OrdinalIgnoreCase)));
    }
}

static partial class HttpContextExtensionsLoggingExtensions
{
    [LoggerMessage(
        EventId = 4801,
        Level = LogLevel.Warning,
        Message = "Could not read HttpContent as string")]
    public static partial void ErrorReadingAsString(this ILogger logger, Exception ex);
}
