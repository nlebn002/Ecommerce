namespace Ecommerce.BasketService.Infrastructure.Messaging.Outbox;

public sealed class OutboxProcessorOptions
{
    public const string SectionName = "Basket:Outbox";

    public int BatchSize { get; set; } = 20;

    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);
}
