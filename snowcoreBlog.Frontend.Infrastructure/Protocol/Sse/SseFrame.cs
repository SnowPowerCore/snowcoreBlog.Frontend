namespace snowcoreBlog.Frontend.Infrastructure.Protocol.Sse;

/// <summary>
/// A single parsed server-sent event frame.
/// </summary>
/// <param name="EventName">The value of the <c>event:</c> field of the frame.</param>
/// <param name="Data">The joined <c>data:</c> payload of the frame (raw JSON for our streams).</param>
public sealed record SseFrame(string EventName, string Data);
