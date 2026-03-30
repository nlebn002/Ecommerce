using System.Text.Json;
using Ecommerce.OrderService.Infrastructure.Persistence;
using MassTransit;

namespace Ecommerce.OrderService.Infrastructure.Messaging.Outbox;

public sealed class OutboxMessagePublisher : IOutboxMessagePublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.General);
    private readonly IPublishEndpoint _publishEndpoint;

    public OutboxMessagePublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        return outboxMessage.Type switch
        {
            OutboxMessageTypes.OrderCreatedV1 => PublishAsync<Ecommerce.Contracts.V1.OrderCreated>(outboxMessage, cancellationToken),
            OutboxMessageTypes.OrderConfirmedV1 => PublishAsync<Ecommerce.Contracts.V1.OrderConfirmed>(outboxMessage, cancellationToken),
            OutboxMessageTypes.OrderCancelledV1 => PublishAsync<Ecommerce.Contracts.V1.OrderCancelled>(outboxMessage, cancellationToken),
            _ => throw new InvalidOperationException($"No publisher is registered for outbox message type '{outboxMessage.Type}'.")
        };
    }

    private Task PublishAsync<TMessage>(OutboxMessage outboxMessage, CancellationToken cancellationToken)
        where TMessage : class, Ecommerce.Contracts.IIntegrationEvent
    {
        var integrationEvent = JsonSerializer.Deserialize<TMessage>(outboxMessage.Payload, SerializerOptions)
            ?? throw new InvalidOperationException($"Outbox message '{outboxMessage.Id}' payload could not be deserialized.");

        return _publishEndpoint.Publish(integrationEvent, context =>
        {
            context.MessageId = integrationEvent.EventId;
            context.CorrelationId = integrationEvent.CorrelationId;
        }, cancellationToken);
    }
}
