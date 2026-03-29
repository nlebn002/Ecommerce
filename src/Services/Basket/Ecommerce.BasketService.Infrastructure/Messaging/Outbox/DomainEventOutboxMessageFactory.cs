using System.Text.Json;
using Ecommerce.BasketService.Domain;
using Ecommerce.BasketService.Infrastructure.Messaging.IntegrationEvents;
using Ecommerce.BasketService.Infrastructure.Persistence;

namespace Ecommerce.BasketService.Infrastructure.Messaging.Outbox;

public sealed class DomainEventOutboxMessageFactory : IDomainEventOutboxMessageFactory
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.General);
    private readonly BasketCheckoutIntegrationEventFactory _basketCheckoutIntegrationEventFactory;
    private readonly TimeProvider _timeProvider;

    public DomainEventOutboxMessageFactory(
        BasketCheckoutIntegrationEventFactory basketCheckoutIntegrationEventFactory,
        TimeProvider timeProvider)
    {
        _basketCheckoutIntegrationEventFactory = basketCheckoutIntegrationEventFactory;
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
                case BasketCheckedOutDomainEvent basketCheckedOut:
                {
                    var integrationEvents = _basketCheckoutIntegrationEventFactory.Create(basketCheckedOut);
                    messages.Add(CreateMessage(OutboxMessageTypes.BasketCheckedOutV1, integrationEvents.V1, occurredOnUtc));
                    messages.Add(CreateMessage(OutboxMessageTypes.BasketCheckedOutV2, integrationEvents.V2, occurredOnUtc));
                    break;
                }
                default:
                    throw new InvalidOperationException(
                        $"No outbox mapping is registered for domain event '{domainEvent.GetType().Name}'.");
            }
        }

        return messages;
    }

    private static OutboxMessage CreateMessage<TMessage>(string type, TMessage message, DateTime occurredOnUtc)
    {
        return OutboxMessage.Create(
            Guid.NewGuid(),
            type,
            JsonSerializer.Serialize(message, SerializerOptions),
            occurredOnUtc);
    }
}
