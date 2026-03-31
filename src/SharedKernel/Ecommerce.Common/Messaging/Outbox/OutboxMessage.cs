namespace Ecommerce.Common.Messaging.Outbox;

public class OutboxMessage
{
    protected OutboxMessage()
    {
    }

    protected OutboxMessage(Guid id, string type, string payload, DateTime occurredOnUtc)
    {
        Id = id;
        Type = type;
        Payload = payload;
        OccurredOnUtc = occurredOnUtc;
    }

    public Guid Id { get; protected set; }

    public string Type { get; protected set; } = string.Empty;

    public string Payload { get; protected set; } = string.Empty;

    public DateTime OccurredOnUtc { get; protected set; }

    public DateTime? ProcessedOnUtc { get; protected set; }

    public DateTime? DiscardedOnUtc { get; protected set; }

    public string? LastError { get; protected set; }

    public int AttemptCount { get; protected set; }

    public static OutboxMessage Create(Guid id, string type, string payload, DateTime occurredOnUtc)
    {
        return new(id, type, payload, occurredOnUtc);
    }

    public void MarkProcessed(DateTime processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        DiscardedOnUtc = null;
        LastError = null;
    }

    public void MarkFailed(string error, int maxRetryAttempts, DateTime failedOnUtc)
    {
        AttemptCount++;
        LastError = error;

        if (AttemptCount >= maxRetryAttempts)
        {
            DiscardedOnUtc = failedOnUtc;
        }
    }
}
