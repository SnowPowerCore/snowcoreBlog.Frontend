using Microsoft.AspNetCore.Components;

namespace snowcoreBlog.Frontend.SharedComponents.Components.Fluent;

/// <summary>
/// A FluentUI-backed status label whose text transitions are animated:
/// the previously displayed message slides up and fades out, while the new
/// message slides up from below and fades in, simultaneously with the exit.
/// <para>
/// Updates are QUEUED, never interrupted: even when the underlying state
/// changes faster than the animation, every distinct message gets its full
/// enter/leave animation cycle, one after another.
/// </para>
/// <para>
/// Feed it by binding the <see cref="Text"/> parameter; the component enqueues
/// every distinct value it observes.
/// </para>
/// </summary>
public sealed partial class AnimatedStatusLabel : ComponentBase, IDisposable
{
    /// <summary>
    /// The exit animation duration; must match the CSS in the scoped stylesheet.
    /// </summary>
    private const int LeaveAnimationMs = 260;

    /// <summary>
    /// The enter animation duration; must match the CSS in the scoped stylesheet.
    /// </summary>
    private const int EnterAnimationMs = 320;

    /// <summary>
    /// How long a freshly displayed message is held on screen (readable dwell
    /// time) before the next queued message starts its transition — also applied
    /// after the last queued message, delaying the parent's exit/navigation.
    /// </summary>
    private const int MessageHoldMs = 800;

    private readonly Queue<string> _pendingMessages = new();
    private readonly CancellationTokenSource _disposalCts = new();

    private string _displayedMessage = string.Empty;
    private string? _lastQueuedMessage;
    private bool _isLeaving;
    private bool _isEntering;
    private bool _isDraining;
    private TaskCompletionSource? _drainedTcs;

    /// <summary>
    /// The current status text. Every DISTINCT value assigned over time is
    /// queued and played through the full animated transition.
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    public void Dispose()
    {
        _drainedTcs?.TrySetResult();
        _disposalCts.Cancel();
        _disposalCts.Dispose();
    }

    /// <summary>
    /// Completes when every currently queued message has finished playing its
    /// animation. Parents must await this BEFORE hiding the component or
    /// navigating away, so that fast incoming statuses are never cut off.
    /// </summary>
    public Task WaitForDrainedAsync()
    {
        if (!_isDraining && _pendingMessages.Count == 0)
        {
            return Task.CompletedTask;
        }

        _drainedTcs ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        return _drainedTcs.Task;
    }

    protected override void OnParametersSet()
    {
        var text = Text ?? string.Empty;
        if (text == _displayedMessage || text == _lastQueuedMessage)
        {
            return;
        }

        _pendingMessages.Enqueue(text);
        _lastQueuedMessage = text;

        if (!_isDraining)
        {
            _isDraining = true;
            _ = DrainQueueAsync();
        }
    }

    private async Task DrainQueueAsync()
    {
        try
        {
            while (_pendingMessages.Count > 0)
            {
                var nextMessage = _pendingMessages.Dequeue();

                // Phase 1: slide the currently displayed message up and fade it out.
                if (_displayedMessage.Length > 0)
                {
                    _isLeaving = true;
                    StateHasChanged();
                    await Task.Delay(LeaveAnimationMs, _disposalCts.Token);
                    _isLeaving = false;
                }

                // Phase 2: slide the new message up from below and fade it in.
                _displayedMessage = nextMessage;
                _isEntering = true;
                StateHasChanged();
                await Task.Delay(EnterAnimationMs, _disposalCts.Token);
                _isEntering = false;
                StateHasChanged();

                // Phase 3: hold the message on screen so it is actually readable
                // before the next transition starts — also after the last message.
                await Task.Delay(MessageHoldMs, _disposalCts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // The component was disposed mid-animation; stop quietly.
        }
        finally
        {
            _isDraining = false;
            _drainedTcs?.TrySetResult();
            _drainedTcs = null;
        }
    }
}
