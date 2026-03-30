using Ecommerce.OrderService.Infrastructure.Persistence;

namespace Ecommerce.OrderService.Infrastructure.Messaging.Outbox;

public interface IOutboxMessagePublisher
{
    Task PublishAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken);
}
