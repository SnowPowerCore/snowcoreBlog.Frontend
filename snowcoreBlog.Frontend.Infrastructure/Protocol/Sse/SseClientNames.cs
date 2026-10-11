namespace snowcoreBlog.Frontend.Infrastructure.Protocol.Sse;

/// <summary>
/// Names of the named <see cref="System.Net.Http.HttpClient"/> registrations
/// used to consume server-sent events streams of the backend services.
/// </summary>
public static class SseClientNames
{
    /// <summary>
    /// The named client targeting the Readers Management backend service
    /// (registered by both the Host and the Client with the same name).
    /// <para>
    /// IMPORTANT: its base address ends with a trailing slash — without it,
    /// HttpClient combines the relative endpoint paths by replacing the last
    /// base address segment ("readers") instead of appending to it.
    /// </para>
    /// </summary>
    public const string ReadersManagement = "ReadersManagementSseClient";
}
