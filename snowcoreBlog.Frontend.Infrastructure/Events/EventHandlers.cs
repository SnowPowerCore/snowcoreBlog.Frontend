using Microsoft.AspNetCore.Components;

namespace snowcoreBlog.Frontend.Infrastructure.Events;

[EventHandler("onverifiedEvent", typeof(AltchaWidgetVerifiedEventArgs),
    enableStopPropagation: true, enablePreventDefault: true)]
public static class EventHandlers { }