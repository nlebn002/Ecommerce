using FluentValidation.Results;

namespace Ecommerce.BasketService.Api.Exceptions;

public sealed class ApiValidationException : Exception
{
    private readonly ValidationResult _validationResult;
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ApiValidationException(ValidationResult validationResult)
        : base("API validation failed.")
    {
        _validationResult = validationResult;
        Errors = _validationResult.Errors
            .GroupBy(error => error.PropertyName, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray(),
                StringComparer.Ordinal);
    }
}

