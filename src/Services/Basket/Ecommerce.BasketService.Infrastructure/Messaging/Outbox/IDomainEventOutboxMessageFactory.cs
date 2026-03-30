using Ecommerce.BasketService.Domain;
using Ecommerce.Common.Messaging.Outbox;

namespace Ecommerce.BasketService.Infrastructure.Messaging.Outbox;

public interface IDomainEventOutboxMessageFactory
{
    IReadOnlyCollection<OutboxMessage> CreateMessages(IReadOnlyCollection<IDomainEvent> domainEvents);
}
