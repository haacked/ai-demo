using System.Diagnostics;

namespace Haack.AIDemoWeb.Library.Clients;

public class LoggingHttpMessageHandler : DelegatingHandler
{
        readonly ILogger<LoggingHttpMessageHandler> _logger;

    /// <summary>
    /// Constructs a <see cref="LoggingHttpMessageHandler"/> with the given <paramref name="logger"/>.
    /// </summary>
    /// <param name="logger"></param>
    public LoggingHttpMessageHandler(ILogger<LoggingHttpMessageHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid().ToString();
        var msg = $"[{id} - Request]";

        LogTrace($"{msg}========Start==========");
        LogTrace($"{msg} {request.Method} {request.RequestUri?.PathAndQuery} {request.RequestUri?.Scheme}/{request.Version}");
        LogTrace($"{msg} Host: {request.RequestUri?.Scheme}://{request.RequestUri?.Host}");

        foreach (var (key, value) in request.Headers)
            LogTrace($"{msg} {key}: {string.Join(", ", value)}");

        if (request.Content != null)
        {
            foreach (var (key, value) in request.Content.Headers)
                LogTrace($"{msg} {key}: {string.Join(", ", value)}");

            if (await request.Content.SafelyReadAsStringAsync(request.Headers, _logger, cancellationToken) is { } requestContent)
            {
                LogTrace($"{msg} Content:");
                LogTrace($"{msg} {requestContent.TruncateToLength(255, appendEllipses: true)}");
            }
        }

        var start = DateTime.Now;

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var end = DateTime.Now;

        LogTrace($"{msg} Duration: {end - start}");
        LogTrace($"{msg}==========End==========");

        msg = $"[{id} - Response]";
        LogTrace($"{msg}=========Start=========");

        Debug.WriteLine($"{msg} {request.RequestUri?.Scheme}/{response.Version} {response.StatusCode} {response.ReasonPhrase}");

        foreach (var (key, value) in response.Headers)
            LogTrace($"{msg} {key}: {string.Join(", ", value)}");

        foreach (var (key, value) in response.Content.Headers)
            LogTrace($"{msg} {key}: {string.Join(", ", value)}");

        start = DateTime.Now;
        if (await response.Content.SafelyReadAsStringAsync(response.Headers, _logger, cancellationToken) is { } responseContent)
        {
            end = DateTime.Now;

            LogTrace($"{msg} Content:");
            LogTrace($"{msg} {responseContent.TruncateToLength(255, appendEllipses: true)}");
            LogTrace($"{msg} Duration: {end - start}");
        }

        LogTrace($"{msg}==========End==========");
        return response;
    }

    void LogTrace(string message)
    {
#pragma warning disable CA1848
#pragma warning disable CA2254
        if (_logger.IsEnabled(LogLevel.Trace))
            _logger.LogTrace(message);
#pragma warning restore CA2254
#pragma warning restore CA1848
    }
}