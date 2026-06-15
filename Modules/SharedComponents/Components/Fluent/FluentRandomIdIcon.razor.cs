using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Extensions;
using Microsoft.FluentUI.AspNetCore.Components.Utilities;

namespace snowcoreBlog.Frontend.SharedComponents.Components.Fluent;

/// <summary>
/// FluentIcon is a component that renders an icon from the Fluent System icon set.
/// </summary>
public partial class FluentRandomIdIcon<RandomIdIcon> : FluentComponentBase where RandomIdIcon : Models.RandomIdIcon, new()
{
    private RandomIdIcon _icon = default!;

    /// <summary />
    protected string? ClassValue => new CssBuilder(Class)
        .Build();

    protected string? StyleValue => new StyleBuilder(Style)
        .AddStyle("width", Width ?? $"{_icon.Width}px", Width != string.Empty)
        .AddStyle("fill", GetIconColor(), () => _icon.Variant != IconVariant.Color)
        .AddStyle("cursor", "pointer", OnClick.HasDelegate)
        .AddStyle("display", "inline-block", !_icon.ContainsSVG)
        .Build();

    //
    // Summary:
    //     Gets or sets the slot where the icon is displayed in.
    [Parameter]
    public string? Slot { get; set; }

    //
    // Summary:
    //     Gets or sets the title for the icon.
    [Parameter]
    public string? Title { get; set; }

    //
    // Summary:
    //     Gets or sets the icon drawing and fill color. Value comes from the Microsoft.FluentUI.AspNetCore.Components.Color
    //     enumeration. Defaults to Accent.
    [Parameter]
    public Color? Color { get; set; }

    //
    // Summary:
    //     Gets or sets the icon drawing and fill color to a custom value. Needs to be formatted
    //     as an HTML hex color string (#rrggbb or #rgb) or CSS variable. ⚠️ Only available
    //     when Color is set to Color.Custom.
    [Parameter]
    public string? CustomColor { get; set; }

    //
    // Summary:
    //     Gets or sets the icon width. If not set, the icon size will be used.
    [Parameter]
    public string? Width { get; set; }

    //
    // Summary:
    //     Gets or sets the Icon object to render.
    [Parameter]
    public RandomIdIcon Value
    {
        get
        {
            return _icon;
        }
        set
        {
            _icon = value;
        }
    }

    //
    // Summary:
    //     Allows for capturing a mouse click on an icon.
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    //
    // Summary:
    //     Gets or sets whether the icon is focusable (adding tabindex="0" and role="button"),
    //     allows the icon to be focused sequentially (generally with the Tab key).
    [Parameter]
    public bool Focusable { get; set; }

    protected virtual Task OnClickHandlerAsync(MouseEventArgs e)
    {
        if (OnClick.HasDelegate)
        {
            return OnClick.InvokeAsync(e);
        }

        return Task.CompletedTask;
    }

    protected virtual Task OnKeyDownAsync(KeyboardEventArgs e)
    {
        if (OnClick.HasDelegate && (e.Key == "Enter" || e.Key == "NumpadEnter"))
        {
            return OnClickHandlerAsync(new MouseEventArgs());
        }

        return Task.CompletedTask;
    }

    protected override void OnParametersSet()
    {
        if (_icon == null)
        {
            _icon = new RandomIdIcon();
        }

        if (!string.IsNullOrEmpty(CustomColor) && Color != Microsoft.FluentUI.AspNetCore.Components.Color.Custom)
        {
            throw new ArgumentException("CustomColor can only be used when Color is set to Color.Custom.");
        }
    }

    //
    // Summary:
    //     Returns FluentIcon.CustomColor, or FluentIcon.Color, or Icon.Color.
    private string GetIconColor()
    {
        string text = Microsoft.FluentUI.AspNetCore.Components.Color.Accent.ToAttributeValue();
        if (Color == Microsoft.FluentUI.AspNetCore.Components.Color.Custom && !string.IsNullOrEmpty(CustomColor))
        {
            return CustomColor;
        }

        if (Color == Microsoft.FluentUI.AspNetCore.Components.Color.Custom && !string.IsNullOrEmpty(_icon.Color))
        {
            return _icon.Color;
        }

        if (Color.HasValue)
        {
            return Color.ToAttributeValue() ?? text;
        }

        if (!string.IsNullOrEmpty(_icon.Color))
        {
            return _icon.Color;
        }

        return text;
    }
}