using System.Reflection;
using Microsoft.AspNetCore.Components.Forms;

namespace snowcoreBlog.Frontend.SharedComponents.Tests.Utilities;

// Helper to access internal static class
internal static class PropertyPathHelperAccessor
{
    private static readonly Type HelperType;
    private static readonly MethodInfo ToFluentPropertyPathMethod;

    static PropertyPathHelperAccessor()
    {
        var assembly = typeof(Validation.FormFluentValidationValidator).Assembly;
        HelperType = assembly.GetType("snowcoreBlog.Frontend.SharedComponents.Utilities.PropertyPathHelper")!;
        ToFluentPropertyPathMethod = HelperType.GetMethod("ToFluentPropertyPath", BindingFlags.Public | BindingFlags.Static)!;
    }

    public static string ToFluentPropertyPath(EditContext editContext, FieldIdentifier fieldIdentifier)
    {
        return (string)ToFluentPropertyPathMethod.Invoke(null, new object[] { editContext, fieldIdentifier })!;
    }
}
