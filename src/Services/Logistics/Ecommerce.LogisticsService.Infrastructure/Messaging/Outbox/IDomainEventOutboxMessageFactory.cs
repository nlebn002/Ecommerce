using Ecommerce.Common.Messaging.Outbox;
using Ecommerce.LogisticsService.Domain;

namespace Ecommerce.LogisticsService.Infrastructure.Messaging.Outbox;

public interface IDomainEventOutboxMessageFactory
{
    IReadOnlyCollection<OutboxMessage> CreateMessages(IReadOnlyCollection<IDomainEvent> domainEvents);
}
