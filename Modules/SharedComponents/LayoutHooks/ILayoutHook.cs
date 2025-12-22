namespace snowcoreBlog.Frontend.SharedComponents.LayoutHooks;

public interface ILayoutHook
{
    int Order { get; }
    Type ComponentType { get; }
}

public sealed record LayoutHook(int Order, Type ComponentType) : ILayoutHook;
