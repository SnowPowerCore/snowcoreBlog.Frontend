using System.Text.Json;
using snowcoreBlog.PublicApi.Utilities.Sse;

namespace snowcoreBlog.Frontend.Infrastructure.Protocol.Sse;

/// <summary>
/// Generic reader for step-progress server-sent events streams emitted by the
/// backend stepified processes. It forwards every intermediate step code to the
/// consumer and returns the terminal <see cref="StepProgressFinalEvent"/>
/// (or <see langword="null"/> when the stream ended without one, e.g. it was
/// aborted mid-way).
/// </summary>
public static class StepProgressStreamReader
{
    /// <summary>
    /// The JSON options used to deserialize the raw frame data payloads.
    /// </summary>
    public static readonly JsonSerializerOptions FrameDataOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Enumerates the frames of a step-progress stream.
    /// </summary>
    /// <param name="frames">The SSE frames of the stream.</param>
    /// <param name="onStepProgress">
    /// A callback receiving every stable, machine-readable step code as it arrives.
    /// </param>
    /// <param name="cancellationToken">Aborts the enumeration.</param>
    /// <returns>The terminal event, or <see langword="null"/> when the stream was aborted mid-way.</returns>
    public static async Task<StepProgressFinalEvent?> ReadAsync(
        IAsyncEnumerable<SseFrame> frames,
        Action<string> onStepProgress,
        CancellationToken cancellationToken = default)
    {
        StepProgressFinalEvent? finalEvent = null;

        await foreach (var frame in frames.WithCancellation(cancellationToken))
        {
            switch (frame.EventName)
            {
                case StepProgressEventConstants.StepProgressEvent:
                    var progress = JsonSerializer.Deserialize<StepProgressEvent>(frame.Data, FrameDataOptions);
                    if (progress is not null)
                    {
                        onStepProgress(progress.Step);
                    }
                    break;

                case StepProgressEventConstants.FinalResultEvent:
                    finalEvent = JsonSerializer.Deserialize<StepProgressFinalEvent>(frame.Data, FrameDataOptions);
                    break;
            }
        }

        return finalEvent;
    }
}
