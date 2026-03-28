using Ecommerce.BasketService.Domain;

namespace Ecommerce.BasketService.Application;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}
