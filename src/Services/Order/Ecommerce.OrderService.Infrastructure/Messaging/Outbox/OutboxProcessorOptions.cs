namespace Ecommerce.OrderService.Infrastructure.Messaging.Outbox;

public sealed class OutboxProcessorOptions
{
    public const string SectionName = "Outbox";

    public int BatchSize { get; set; } = 20;

    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);
}
