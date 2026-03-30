namespace Ecommerce.OrderService.Domain;

public enum OrderErrorType
{
    Validation,
    NotFound,
    Conflict
}

public enum OrderErrorCode
{
    RequestValidationFailed,
    OrderNotFound,
    OrderInactive,
    ShipmentAlreadyProcessed,
    InvalidOrderState,
    InvalidShippingPrice,
    InvalidCustomerId,
    InvalidOrderItem,
    ConcurrencyConflict
}

public sealed class OrderException : Exception
{
    public OrderException(
        OrderErrorType type,
        OrderErrorCode code,
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        Type = type;
        Code = code;
        Errors = errors;
    }

    public OrderErrorType Type { get; }

    public OrderErrorCode Code { get; }

    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    public static OrderException Validation(OrderErrorCode code, string field, string message)
    {
        return new(
            OrderErrorType.Validation,
            code,
            message,
            new Dictionary<string, string[]>
            {
                [field] = [message]
            });
    }

    public static OrderException Validation(
        OrderErrorCode code,
        string message,
        IReadOnlyDictionary<string, string[]> errors)
    {
        return new(OrderErrorType.Validation, code, message, errors);
    }

    public static OrderException NotFound(OrderErrorCode code, string message)
    {
        return new(OrderErrorType.NotFound, code, message);
    }

    public static OrderException Conflict(OrderErrorCode code, string message)
    {
        return new(OrderErrorType.Conflict, code, message);
    }
}
