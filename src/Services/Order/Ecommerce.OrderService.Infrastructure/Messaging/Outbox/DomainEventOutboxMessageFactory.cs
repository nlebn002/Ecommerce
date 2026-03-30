using System.Text.Json;
using Ecommerce.Common.Messaging.Outbox;
using Ecommerce.OrderService.Domain;
using Ecommerce.OrderService.Infrastructure.Messaging.IntegrationEvents;

namespace Ecommerce.OrderService.Infrastructure.Messaging.Outbox;

public sealed class DomainEventOutboxMessageFactory : IDomainEventOutboxMessageFactory
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.General);
    private readonly OrderIntegrationEventFactory _orderIntegrationEventFactory;
    private readonly TimeProvider _timeProvider;

    public DomainEventOutboxMessageFactory(
        OrderIntegrationEventFactory orderIntegrationEventFactory,
        TimeProvider timeProvider)
    {
        _orderIntegrationEventFactory = orderIntegrationEventFactory;
        _timeProvider = timeProvider;
    }

    public IReadOnlyCollection<OutboxMessage> CreateMessages(IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        var occurredOnUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var messages = new List<OutboxMessage>();

        foreach (var domainEvent in domainEvents)
        {
            switch (domainEvent)
            {
                case OrderCreatedDomainEvent orderCreated:
                    messages.Add(CreateMessage(OutboxMessageTypes.OrderCreatedV1, _orderIntegrationEventFactory.Create(orderCreated), occurredOnUtc));
                    break;
                case OrderConfirmedDomainEvent orderConfirmed:
                    messages.Add(CreateMessage(OutboxMessageTypes.OrderConfirmedV1, _orderIntegrationEventFactory.Create(orderConfirmed), occurredOnUtc));
                    break;
                case OrderCancelledDomainEvent orderCancelled:
                    messages.Add(CreateMessage(OutboxMessageTypes.OrderCancelledV1, _orderIntegrationEventFactory.Create(orderCancelled), occurredOnUtc));
                    break;
                default:
                    throw new InvalidOperationException($"No outbox mapping is registered for domain event '{domainEvent.GetType().Name}'.");
            }
        }

        return messages;
    }

    private static OutboxMessage CreateMessage<TMessage>(string type, TMessage message, DateTime occurredOnUtc)
    {
        return OutboxMessage.Create(Guid.NewGuid(), type, JsonSerializer.Serialize(message, SerializerOptions), occurredOnUtc);
    }
}
