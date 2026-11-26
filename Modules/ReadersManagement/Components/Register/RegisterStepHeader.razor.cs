using Microsoft.AspNetCore.Components;
using snowcoreBlog.Frontend.Core.Resources;

namespace snowcoreBlog.Frontend.ReadersManagement.Components.Register;

/// <summary>
/// OOBE-style header for a registration step page: "Step X of Y" caption,
/// a thin progress bar and the step title / subtitle.
/// </summary>
public partial class RegisterStepHeader
{
    [Parameter, EditorRequired]
    public int Step { get; set; }

    [Parameter, EditorRequired]
    public int TotalSteps { get; set; }

    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string Subtitle { get; set; } = string.Empty;

    private string StepCaption =>
        string.Format(TranslationResources.RegisterStepCounterLabel, Step, TotalSteps);
}
