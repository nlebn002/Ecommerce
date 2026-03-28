using Ecommerce.Basket.Domain;

namespace Ecommerce.Basket.Application;

public interface IBasketCheckoutPublisher
{
    Task PublishAsync(Basket basket, CancellationToken cancellationToken);
}
