namespace Ecommerce.BasketService.Application;

public sealed class BasketNotFoundException : Exception
{
    public BasketNotFoundException(string message)
        : base(message)
    {
    }
}

