namespace Ecommerce.Contracts;

public interface IIntegrationEvent
{
    Guid EventId { get; init; }

    Guid CorrelationId { get; init; }

    Guid? CausationId { get; init; }

    DateTimeOffset Timestamp { get; init; }

    string Version { get; }
}
