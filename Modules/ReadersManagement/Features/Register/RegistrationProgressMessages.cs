using snowcoreBlog.Frontend.Core.Resources;
using snowcoreBlog.PublicApi.Constants;

namespace snowcoreBlog.Frontend.ReadersManagement.Features.Register;

/// <summary>
/// Maps stable, machine-readable reader account registration step codes
/// (pushed by the backend through server-sent events) to localized,
/// user-friendly status messages.
/// </summary>
public static class RegistrationProgressMessages
{
    /// <summary>
    /// Translates a step code into a localized status message.
    /// Unknown codes fall back to a generic processing message.
    /// </summary>
    /// <param name="stepCode">The machine-readable step code from a <c>step-progress</c> SSE event.</param>
    public static string ForStep(string? stepCode) => stepCode switch
    {
        ReaderAccountStepProgressCodes.ValidatingEmailDomain => TranslationResources.RegistrationStepValidatingEmailDomain,
        ReaderAccountStepProgressCodes.CheckingAccountExists => TranslationResources.RegistrationStepCheckingAccountExists,
        ReaderAccountStepProgressCodes.CheckingPendingRegistration => TranslationResources.RegistrationStepCheckingPendingRegistration,
        ReaderAccountStepProgressCodes.CheckingNickNameAvailability => TranslationResources.RegistrationStepCheckingNickNameAvailability,
        ReaderAccountStepProgressCodes.CreatingTempUser => TranslationResources.RegistrationStepCreatingTempUser,
        ReaderAccountStepProgressCodes.CreatingUserAccount => TranslationResources.RegistrationStepCreatingUserAccount,
        ReaderAccountStepProgressCodes.CreatingReaderProfile => TranslationResources.RegistrationStepCreatingReaderProfile,
        _ => TranslationResources.RegistrationStepWorkingGeneric
    };
}
