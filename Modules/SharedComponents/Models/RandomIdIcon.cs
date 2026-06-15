using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Extensions;
using Microsoft.FluentUI.AspNetCore.Components.Utilities;

namespace snowcoreBlog.Frontend.SharedComponents.Models;

/// <summary>
/// FluentUI Icon content.
/// </summary>
public class RandomIdIcon : IconInfo
{
    //
    // Summary:
    //     Gets the content of the icon: SVG path.
    public virtual string Content { get; }

    //
    // Summary:
    //     Gets the color of the icon.
    internal virtual string? Color { get; private set; }

    //
    // Summary:
    //     Gets the width of the icon.
    protected internal virtual int Width
    {
        get
        {
            if (Size != IconSize.Custom)
            {
                return (int)Size;
            }

            return 20;
        }
    }

    //
    // Summary:
    //     Returns true if the icon contains a SVG content.
    protected internal bool ContainsSVG
    {
        get
        {
            if (!string.IsNullOrEmpty(Content))
            {
                if (!Content.StartsWith("<path ") && !Content.StartsWith("<rect ") && !Content.StartsWith("<g ") && !Content.StartsWith("<circle "))
                {
                    return Content.StartsWith("<mark ");
                }

                return true;
            }

            return false;
        }
    }

    //
    // Summary:
    //     Please use the constructor including parameters.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    public RandomIdIcon()
        : this(string.Empty, IconVariant.Regular, IconSize.Size24, string.Empty)
    {
        throw new ArgumentNullException("Please use the constructor including parameters.");
    }

    //
    // Summary:
    //     Initializes a new instance of the Microsoft.FluentUI.AspNetCore.Components.Icon
    //     class.
    //
    // Parameters:
    //   name:
    //     Microsoft.FluentUI.AspNetCore.Components.IconInfo.Name
    //
    //   variant:
    //     Microsoft.FluentUI.AspNetCore.Components.IconInfo.Variant
    //
    //   size:
    //     Microsoft.FluentUI.AspNetCore.Components.IconInfo.Size
    //
    //   content:
    //     Microsoft.FluentUI.AspNetCore.Components.Icon.Content
    public RandomIdIcon(string name, IconVariant variant, IconSize size, string content)
    {
        Name = name;
        Variant = variant;
        Size = size;
        Content = string.Format(content, GetHashCode().ToString());
    }

    //
    // Summary:
    //     Sets the color of the icon.
    //
    // Parameters:
    //   color:
    public virtual RandomIdIcon WithColor(string? color)
    {
        if (!string.IsNullOrEmpty(color))
        {
            Color = color;
        }

        return this;
    }

    //
    // Summary:
    //     Sets the color of the icon.
    //
    // Parameters:
    //   color:
    public virtual RandomIdIcon WithColor(Color color)
    {
        Color = color.ToAttributeValue();
        return this;
    }

    //
    // Summary:
    //     Inverse the color of the icon, if the accentContainer is true, and is not set
    //     (Microsoft.FluentUI.AspNetCore.Components.Icon.Color overrides this accent color).
    //
    //
    // Parameters:
    //   accentContainer:
    internal RandomIdIcon InverseColor(bool accentContainer)
    {
        if (accentContainer && Color == null)
        {
            Color = Microsoft.FluentUI.AspNetCore.Components.Color.Lightweight.ToAttributeValue();
        }

        return this;
    }

    //
    // Summary:
    //     Gets the HTML markup of the icon.
    public virtual MarkupString ToMarkup(string? size = null, string? color = null)
    {
        if (Size != IconSize.Custom && ContainsSVG)
        {
            var value = size ?? $"{Size}px";
            var value2 = color ?? Color ?? "var(--accent-fill-rest)";
            return new MarkupString($"<svg viewBox=\"0 0 {Size} {Size}\" width=\"{value}\" fill=\"{value2}\" style=\"background-color: var(--neutral-layer-1); width: {value};\" aria-hidden=\"true\">{Content}</svg>");
        }

        if (string.IsNullOrEmpty(size) && string.IsNullOrEmpty(color))
        {
            return new MarkupString(Content);
        }

        var value3 = new StyleBuilder()
            .AddStyle("display", "inline-block")
            .AddStyle("fill", color, !string.IsNullOrEmpty(color))
            .AddStyle("width", size, !string.IsNullOrEmpty(size))
            .Build();
        return new MarkupString($"<div style=\"{value3}\">{Content}</div>");
    }

    //
    // Summary:
    //     Gets the data URI of the icon.
    public virtual string ToDataUri(string? size = null, string? color = null)
    {
        var value = ToMarkup(size, color).Value;
        value = value.Contains("http://www.w3.org/2000/svg") ? value : value.Replace("<svg ", "<svg xmlns=\"http://www.w3.org/2000/svg\" ");
        var text = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        return "data:image/svg+xml;base64," + text;
    }

    //
    // Summary:
    //     Returns an icon instance.
    public static TIcon FromType<TIcon>() where TIcon : Icon, new()
    {
        return new TIcon();
    }

    //
    // Summary:
    //     Returns an icon from an image source.
    //
    // Parameters:
    //   imageSource:
    public static IconFromImage FromImageUrl(string imageSource)
    {
        return new IconFromImage(imageSource);
    }
}