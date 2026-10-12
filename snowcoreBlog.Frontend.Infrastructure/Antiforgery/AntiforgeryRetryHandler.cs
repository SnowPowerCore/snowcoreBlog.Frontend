using System.Net;
using Microsoft.Extensions.Logging;
using snowcoreBlog.Frontend.Core.Interfaces.Antiforgery;
using snowcoreBlog.PublicApi.Constants;

namespace snowcoreBlog.Frontend.Infrastructure.Antiforgery;

/// <summary>
/// Central antiforgery recovery for outgoing service calls, so pages and
/// components no longer have to handle the "first submit has no token" /
/// "token went stale" cases themselves:
/// <para>
/// 1. Proactive: when the outgoing request carries no (or an empty) antiforgery
///    header, a token is fetched through the owning module's
///    <see cref="IAntiforgeryTokenRefresher"/> and stamped onto the request
///    before the first attempt.
/// </para>
/// <para>
/// 2. Reactive: a bare HTTP 400 with an empty body is how the antiforgery
///    middleware rejects a stale/rotated token (FastEndpoints validation and
///    the captcha pre-processor reject with JSON bodies instead). In that case
///    the token is force-refreshed and the buffered request is replayed once.
///    Business validation failures are never replayed.
/// </para>
/// </summary>
/// <remarks>
/// Register this handler as the FIRST message handler of a client (outermost)
/// so replays pass through the remaining pipeline (cookie propagation etc.).
/// The request body is buffered up front, making both attempts possible; the
/// replay itself goes through <see cref="DelegatingHandler.SendAsync"/>'s inner
/// pipeline, so the handler can never recurse on itself.
/// </remarks>
public sealed class AntiforgeryRetryHandler(
    IAntiforgeryTokenRefresher tokenRefresher,
    ILogger<AntiforgeryRetryHandler>? logger = null) : DelegatingHandler
{
    /// <summary>Wire format of the antiforgery header shared with the backend services.</summary>
    public const string AntiforgeryHeaderName = HeaderKeyConstants.AntiforgeryHeader;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var hadToken = TryGetToken(request, out _);
        if (!hadToken)
        {
            var stamped = await tokenRefresher.GetTokenAsync(forceRefresh: false, cancellationToken);
            if (!string.IsNullOrEmpty(stamped))
            {
                request.Headers.Remove(AntiforgeryHeaderName);
                request.Headers.Add(AntiforgeryHeaderName, stamped);
                hadToken = true;
            }
        }

        // Buffer the body once and swap it in as replayable content, so the
        // first send cannot consume the caller's content stream.
        byte[]? bufferedBody = null;
        if (request.Content is not null)
        {
            bufferedBody = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            var bufferedContent = new ByteArrayContent(bufferedBody);
            foreach (var header in request.Content.Headers)
                bufferedContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            request.Content = bufferedContent;
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode is not HttpStatusCode.BadRequest || !hadToken)
            return response;

        // Only a bare 400 (empty body) is the antiforgery middleware rejecting
        // the token; anything else is a business answer that must not replay.
        var failureBody = response.Content is null
            ? string.Empty
            : await ReadSnippetAsync(response.Content, cancellationToken);
        if (!string.IsNullOrWhiteSpace(failureBody))
            return response;

        var refreshed = await tokenRefresher.GetTokenAsync(forceRefresh: true, cancellationToken);
        if (string.IsNullOrEmpty(refreshed))
            return response;

        logger?.LogWarning(
            "Antiforgery token rejected (400) for {Method} {Uri}; refreshing it and replaying once.",
            request.Method, request.RequestUri);

        using var replay = CloneRequest(request, refreshed, bufferedBody);
        var replayResponse = await base.SendAsync(replay, cancellationToken);
        response.Dispose();
        return replayResponse;
    }

    private static bool TryGetToken(HttpRequestMessage request, out string? token)
    {
        token = null;
        if (!request.Headers.TryGetValues(AntiforgeryHeaderName, out var values))
            return false;

        token = values.FirstOrDefault(static v => !string.IsNullOrWhiteSpace(v));
        return token is not null;
    }

    private static async Task<string> ReadSnippetAsync(HttpContent content, CancellationToken cancellationToken)
    {
        try
        {
            // Antiforgery / problem-detail bodies are tiny; cap defensively anyway.
            var bytes = await content.ReadAsByteArrayAsync(cancellationToken);
            return bytes.Length <= 4096
                ? System.Text.Encoding.UTF8.GetString(bytes)
                : System.Text.Encoding.UTF8.GetString(bytes, 0, 4096);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return string.Empty;
        }
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage original, string refreshedToken, byte[]? bufferedBody)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri)
        {
            Version = original.Version,
            VersionPolicy = original.VersionPolicy
        };

        foreach (var header in original.Headers)
        {
            if (string.Equals(header.Key, AntiforgeryHeaderName, StringComparison.OrdinalIgnoreCase))
                continue;
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        clone.Headers.Add(AntiforgeryHeaderName, refreshedToken);

        if (original.Content is not null)
        {
            var content = new ByteArrayContent(bufferedBody ?? []);
            foreach (var header in original.Content.Headers)
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            clone.Content = content;
        }

        return clone;
    }
}