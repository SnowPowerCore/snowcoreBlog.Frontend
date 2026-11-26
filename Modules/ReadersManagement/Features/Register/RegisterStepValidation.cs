using FluentValidation;
using snowcoreBlog.PublicApi.BusinessObjects.Dto;

namespace snowcoreBlog.Frontend.ReadersManagement.Features.Register;

/// <summary>
/// Per-step validation for the multi-page (OOBE-style) registration flow.
/// Runs the shared <see cref="RequestCreateReaderAccountValidator"/> but keeps
/// only the errors that belong to the properties shown on the current step,
/// so earlier / later steps never surface their messages mid-flow.
/// </summary>
public static class RegisterStepValidation
{
    public static IReadOnlyList<string> ValidateStep(
        IValidator<RequestCreateReaderAccountDto> validator,
        RequestCreateReaderAccountDto model,
        IReadOnlySet<string> stepProperties)
    {
        var result = validator.Validate(model);

        return [.. result.Errors
            .Where(error => stepProperties.Contains(error.PropertyName))
            .Select(error => error.ErrorMessage)];
    }
}
