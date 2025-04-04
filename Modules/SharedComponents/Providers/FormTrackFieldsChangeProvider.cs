using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace snowcoreBlog.Frontend.SharedComponents.Providers;

public class FormTrackFieldsChangeProvider : ComponentBase, IDisposable
{
    [CascadingParameter] private EditContext? CurrentEditContext { get; set; }

    [Parameter] public EventCallback<FieldChangedEventArgs> OnFieldChanged { get; set; } = EventCallback<FieldChangedEventArgs>.Empty;

    protected override void OnInitialized()
    {
        if (CurrentEditContext == null)
        {
            throw new InvalidOperationException($"{nameof(FormTrackFieldsChangeProvider)} requires a cascading " +
                                                $"parameter of type {nameof(EditContext)}. For example, you can use {nameof(FormTrackFieldsChangeProvider)} " +
                                                $"inside an {nameof(EditForm)}.");
        }

        CurrentEditContext.OnFieldChanged += EditContextOnFieldChanged;
    }

    private void EditContextOnFieldChanged(object? sender, FieldChangedEventArgs args) =>
        _ = OnFieldChanged.InvokeAsync(args);
        
    void IDisposable.Dispose()
    {
        if (CurrentEditContext is not default(EditContext))
            CurrentEditContext.OnFieldChanged -= EditContextOnFieldChanged;
    }
}