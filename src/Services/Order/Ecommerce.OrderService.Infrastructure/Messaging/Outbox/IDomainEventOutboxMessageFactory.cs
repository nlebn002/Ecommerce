using Ecommerce.OrderService.Domain;
using Ecommerce.OrderService.Infrastructure.Persistence;

namespace Ecommerce.OrderService.Infrastructure.Messaging.Outbox;

public interface IDomainEventOutboxMessageFactory
{
    IReadOnlyCollection<OutboxMessage> CreateMessages(IReadOnlyCollection<IDomainEvent> domainEvents);
}
