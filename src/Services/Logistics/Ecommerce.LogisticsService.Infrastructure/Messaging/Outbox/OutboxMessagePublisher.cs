using System.Text.Json;
using Ecommerce.Common.Messaging.Outbox;
using MassTransit;

namespace Ecommerce.LogisticsService.Infrastructure.Messaging.Outbox;

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
            OutboxMessageTypes.ShipmentReservedV1 => PublishAsync<Ecommerce.Contracts.V1.ShipmentReserved>(outboxMessage, cancellationToken),
            OutboxMessageTypes.ShipmentRepricedV1 => PublishAsync<Ecommerce.Contracts.V1.ShipmentRepriced>(outboxMessage, cancellationToken),
            OutboxMessageTypes.ShipmentFailedV1 => PublishAsync<Ecommerce.Contracts.V1.ShipmentFailed>(outboxMessage, cancellationToken),
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
