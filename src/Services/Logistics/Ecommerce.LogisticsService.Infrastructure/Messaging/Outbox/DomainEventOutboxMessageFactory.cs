using System.Text.Json;
using Ecommerce.Common.Messaging.Outbox;
using Ecommerce.LogisticsService.Domain;
using Ecommerce.LogisticsService.Infrastructure.Messaging.IntegrationEvents;

namespace Ecommerce.LogisticsService.Infrastructure.Messaging.Outbox;

public sealed class DomainEventOutboxMessageFactory : IDomainEventOutboxMessageFactory
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.General);
    private readonly LogisticsIntegrationEventFactory _logisticsIntegrationEventFactory;
    private readonly TimeProvider _timeProvider;

    public DomainEventOutboxMessageFactory(
        LogisticsIntegrationEventFactory logisticsIntegrationEventFactory,
        TimeProvider timeProvider)
    {
        _logisticsIntegrationEventFactory = logisticsIntegrationEventFactory;
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
                case ShipmentReservedDomainEvent shipmentReserved:
                    messages.Add(CreateMessage(OutboxMessageTypes.ShipmentReservedV1, _logisticsIntegrationEventFactory.Create(shipmentReserved), occurredOnUtc));
                    break;
                case ShipmentRepricedDomainEvent shipmentRepriced:
                    messages.Add(CreateMessage(OutboxMessageTypes.ShipmentRepricedV1, _logisticsIntegrationEventFactory.Create(shipmentRepriced), occurredOnUtc));
                    break;
                case ShipmentFailedDomainEvent shipmentFailed:
                    messages.Add(CreateMessage(OutboxMessageTypes.ShipmentFailedV1, _logisticsIntegrationEventFactory.Create(shipmentFailed), occurredOnUtc));
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
