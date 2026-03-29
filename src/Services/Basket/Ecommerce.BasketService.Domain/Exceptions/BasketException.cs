namespace Ecommerce.BasketService.Domain;

public enum BasketErrorType
{
    Validation,
    NotFound,
    Conflict
}

public enum BasketErrorCode
{
    RequestValidationFailed,
    BasketNotFound,
    BasketInactive,
    BasketEmpty,
    BasketItemNotFound,
    InvalidQuantity,
    InvalidUnitPrice,
    InvalidCustomerId,
    ConcurrencyConflict
}

public sealed class BasketException : Exception
{
    public BasketException(
        BasketErrorType type,
        BasketErrorCode code,
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        Type = type;
        Code = code;
        Errors = errors;
    }

    public BasketErrorType Type { get; }

    public BasketErrorCode Code { get; }

    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    public static BasketException Validation(BasketErrorCode code, string field, string message)
    {
        return new BasketException(
            BasketErrorType.Validation,
            code,
            message,
            new Dictionary<string, string[]>
            {
                [field] = [message]
            });
    }

    public static BasketException Validation(
        BasketErrorCode code,
        string message,
        IReadOnlyDictionary<string, string[]> errors)
    {
        return new BasketException(BasketErrorType.Validation, code, message, errors);
    }

    public static BasketException NotFound(BasketErrorCode code, string message)
    {
        return new BasketException(BasketErrorType.NotFound, code, message);
    }

    public static BasketException Conflict(BasketErrorCode code, string message)
    {
        return new BasketException(BasketErrorType.Conflict, code, message);
    }
}
