namespace snowcoreBlog.Frontend.SharedComponents.LayoutHooks;

public interface ILayoutHook
{
    int Order { get; }
    Type ComponentType { get; }
}
