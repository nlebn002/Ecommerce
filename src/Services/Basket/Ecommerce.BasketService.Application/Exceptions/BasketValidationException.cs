namespace Ecommerce.BasketService.Application;

public sealed class BasketValidationException : Exception
{
    public BasketValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Basket validation failed.")
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public static BasketValidationException For(string field, string message)
    {
        return new BasketValidationException(new Dictionary<string, string[]>
        {
            [field] = [message]
        });
    }
}

