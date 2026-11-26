using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace snowcoreBlog.Frontend.ReadersManagement.Components.Register;

/// <summary>
/// Windows-OOBE-style two-pane card body: a large centered icon on the left,
/// the step header / form fields / buttons on the right.
/// Set <see cref="Step"/> to render the progress header; omit it for the
/// stepless welcome screen.
/// </summary>
public partial class RegisterOobeLayout
{
    [Parameter, EditorRequired]
    public Icon Icon { get; set; } = default!;

    /// <summary>1-based step number; leave null on the welcome screen.</summary>
    [Parameter]
    public int? Step { get; set; }

    [Parameter]
    public int TotalSteps { get; set; }

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string Subtitle { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; } = default!;
}
