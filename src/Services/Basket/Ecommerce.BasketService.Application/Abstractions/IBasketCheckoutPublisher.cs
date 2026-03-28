using Ecommerce.BasketService.Domain;

namespace Ecommerce.BasketService.Application;

public interface IBasketCheckoutPublisher
{
    Task PublishAsync(Basket basket, CancellationToken cancellationToken);
}

