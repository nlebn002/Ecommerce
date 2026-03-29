using Ecommerce.BasketService.Infrastructure.Persistence;

namespace Ecommerce.BasketService.Infrastructure.Messaging.Outbox;

public interface IOutboxMessagePublisher
{
    Task PublishAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken);
}
