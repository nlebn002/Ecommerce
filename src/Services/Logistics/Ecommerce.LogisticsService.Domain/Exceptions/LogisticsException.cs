namespace Ecommerce.LogisticsService.Domain;

public enum LogisticsErrorType
{
    Validation,
    NotFound,
    Conflict
}

public enum LogisticsErrorCode
{
    RequestValidationFailed,
    ShipmentNotFound,
    ShipmentAlreadyFinalized,
    InvalidShipmentState,
    InvalidOrderId,
    InvalidCarrier,
    InvalidShippingPrice,
    ConcurrencyConflict
}

public sealed class LogisticsException : Exception
{
    public LogisticsException(
        LogisticsErrorType type,
        LogisticsErrorCode code,
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        Type = type;
        Code = code;
        Errors = errors;
    }

    public LogisticsErrorType Type { get; }

    public LogisticsErrorCode Code { get; }

    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    public static LogisticsException Validation(LogisticsErrorCode code, string field, string message)
    {
        return new(
            LogisticsErrorType.Validation,
            code,
            message,
            new Dictionary<string, string[]>
            {
                [field] = [message]
            });
    }

    public static LogisticsException Validation(
        LogisticsErrorCode code,
        string message,
        IReadOnlyDictionary<string, string[]> errors)
    {
        return new(LogisticsErrorType.Validation, code, message, errors);
    }

    public static LogisticsException NotFound(LogisticsErrorCode code, string message)
    {
        return new(LogisticsErrorType.NotFound, code, message);
    }

    public static LogisticsException Conflict(LogisticsErrorCode code, string message)
    {
        return new(LogisticsErrorType.Conflict, code, message);
    }
}
