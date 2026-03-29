namespace Ecommerce.BasketService.Domain;

public sealed class BasketDomainException : Exception
{
    public BasketDomainException(string errorCode, string field, string message)
        : base(message)
    {
        ErrorCode = errorCode;
        Field = field;
    }

    public string ErrorCode { get; }

    public string Field { get; }
}
