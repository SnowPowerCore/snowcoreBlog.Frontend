using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace snowcoreBlog.Frontend.Infrastructure.Protocol.Sse;

/// <summary>
/// Generic, domain-agnostic consumption of server-sent events streams over POST
/// requests. The parser is deliberately minimal: it understands the
/// <c>event:</c>, <c>data:</c>, <c>id:</c> and <c>retry:</c> fields, tolerates
/// comments / keep-alive lines and yields one <see cref="SseFrame"/> per frame.
/// </summary>
public static class SseClientExtensions
{
    private const string MessageEventName = "message";

    private static readonly JsonSerializerOptions RequestSerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// POSTs <paramref name="requestBody"/> and lazily enumerates the
    /// server-sent events frames of the streaming response.
    /// </summary>
    /// <param name="httpClient">The HTTP client (a streaming-capable named client is expected).</param>
    /// <param name="requestUri">The relative or absolute URI of the SSE endpoint.</param>
    /// <param name="requestBody">The JSON request payload (may be <see langword="null"/>).</param>
    /// <param name="configureRequest">Optional extra configuration (headers etc.).</param>
    /// <param name="cancellationToken">Aborts the request and the stream enumeration.</param>
    public static async IAsyncEnumerable<SseFrame> PostServerSentEventsAsync(
        this HttpClient httpClient,
        string? requestUri,
        object? requestBody,
        Action<HttpRequestMessage>? configureRequest = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        if (requestBody is not null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody, RequestSerializerOptions),
                Encoding.UTF8,
                MediaTypeNames.Application.Json);
        }

        configureRequest?.Invoke(request);

        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var eventStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(eventStream);

        var eventName = MessageEventName;
        var data = new StringBuilder();

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (line.Length == 0)
            {
                // An empty line terminates a frame.
                if (data.Length > 0)
                {
                    yield return new SseFrame(eventName, data.ToString());
                    eventName = MessageEventName;
                    data.Clear();
                }

                continue;
            }

            if (line.StartsWith(':'))
            {
                // A comment / keep-alive line.
                continue;
            }

            if (line.StartsWith("data:"))
            {
                // Per the SSE spec, a data field may span multiple lines;
                // they are joined back with line feeds by the parser.
                if (data.Length > 0)
                {
                    data.Append('\n');
                }

                data.Append(line.AsSpan("data:".Length).TrimStart());
            }
            else if (line.StartsWith("event:"))
            {
                eventName = line["event:".Length..].Trim();
            }

            // "id:" and "retry:" fields are not needed by the consumers.
        }

        // Flush a trailing frame that was not terminated by a blank line.
        if (data.Length > 0)
        {
            yield return new SseFrame(eventName, data.ToString());
        }
    }
}
