using FluentValidation;
using FluentValidation.Internal;

namespace snowcoreBlog.Frontend.SharedComponents.Validation;

internal class IntersectingCompositeValidatorSelector(IEnumerable<IValidatorSelector> selectors) : IValidatorSelector
{

    public bool CanExecute(IValidationRule rule, string propertyPath, IValidationContext context) =>
        selectors.All(s => s.CanExecute(rule, propertyPath, context));
}