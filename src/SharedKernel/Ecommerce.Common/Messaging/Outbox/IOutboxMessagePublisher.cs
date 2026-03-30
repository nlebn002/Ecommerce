namespace Ecommerce.Common.Messaging.Outbox;

public interface IOutboxMessagePublisher
{
    Task PublishAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken);
}
