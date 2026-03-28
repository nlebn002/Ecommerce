namespace Ecommerce.Contracts;

public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public Guid CorrelationId { get; init; }

    public Guid? CausationId { get; init; }

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public virtual string Version { get; init; } = "1.0";
}
