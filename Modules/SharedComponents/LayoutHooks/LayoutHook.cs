namespace snowcoreBlog.Frontend.SharedComponents.LayoutHooks;

public sealed record LayoutHook(int Order, Type ComponentType) : ILayoutHook;
