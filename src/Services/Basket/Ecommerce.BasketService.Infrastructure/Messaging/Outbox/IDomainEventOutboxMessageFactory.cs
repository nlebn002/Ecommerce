using Ecommerce.BasketService.Domain;
using Ecommerce.BasketService.Infrastructure.Persistence;

namespace Ecommerce.BasketService.Infrastructure.Messaging.Outbox;

public interface IDomainEventOutboxMessageFactory
{
    IReadOnlyCollection<OutboxMessage> CreateMessages(IReadOnlyCollection<IDomainEvent> domainEvents);
}
