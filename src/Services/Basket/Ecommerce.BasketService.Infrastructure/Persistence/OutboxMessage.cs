namespace Ecommerce.BasketService.Infrastructure.Persistence;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    private OutboxMessage(Guid id, string type, string payload, DateTime occurredOnUtc)
    {
        Id = id;
        Type = type;
        Payload = payload;
        OccurredOnUtc = occurredOnUtc;
    }

    public Guid Id { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Payload { get; private set; } = string.Empty;

    public DateTime OccurredOnUtc { get; private set; }

    public DateTime? ProcessedOnUtc { get; private set; }

    public string? LastError { get; private set; }

    public int AttemptCount { get; private set; }

    public static OutboxMessage Create(Guid id, string type, string payload, DateTime occurredOnUtc)
    {
        return new OutboxMessage(id, type, payload, occurredOnUtc);
    }

    public void MarkProcessed(DateTime processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        LastError = null;
    }

    public void MarkFailed(string error)
    {
        AttemptCount++;
        LastError = error;
    }
}
