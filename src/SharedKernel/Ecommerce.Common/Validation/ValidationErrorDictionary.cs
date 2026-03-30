namespace Ecommerce.Common.Validation;

public static class ValidationErrorDictionary
{
    public static IReadOnlyDictionary<string, string[]> Create<TError>(
        IEnumerable<TError> errors,
        Func<TError, string> propertyNameSelector,
        Func<TError, string> errorMessageSelector)
    {
        return errors
            .GroupBy(propertyNameSelector, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(errorMessageSelector).ToArray(),
                StringComparer.Ordinal);
    }
}
