namespace Ecommerce.BasketService.Application;

public sealed class BasketConflictException : Exception
{
    public BasketConflictException(string message)
        : base(message)
    {
    }
}

