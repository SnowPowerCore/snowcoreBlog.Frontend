using Microsoft.AspNetCore.Components;

namespace snowcoreBlog.Frontend.SharedComponents.Services;

public class OverlayService
{
    public event Action? OnChange;

    private RenderFragment? _content;

    public RenderFragment? Content => _content;

    public bool Visible => _content is not default(RenderFragment);

    public void Show(RenderFragment content)
    {
        _content = content;
        OnChange?.Invoke();
    }

    public void Hide()
    {
        _content = default;
        OnChange?.Invoke();
    }
}
