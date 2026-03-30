using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Common.Messaging.Outbox;

public sealed class OutboxMessageProcessor
{
    private readonly IOutboxDbContext _dbContext;
    private readonly IOutboxMessagePublisher _outboxMessagePublisher;
    private readonly TimeProvider _timeProvider;

    public OutboxMessageProcessor(
        IOutboxDbContext dbContext,
        IOutboxMessagePublisher outboxMessagePublisher,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _outboxMessagePublisher = outboxMessagePublisher;
        _timeProvider = timeProvider;
    }

    public async Task<int> ProcessPendingMessagesAsync(int batchSize, CancellationToken cancellationToken)
    {
        var pendingMessages = await _dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null)
            .OrderBy(message => message.OccurredOnUtc)
            .ThenBy(message => message.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        var processedCount = 0;

        foreach (var pendingMessage in pendingMessages)
        {
            try
            {
                await _outboxMessagePublisher.PublishAsync(pendingMessage, cancellationToken);
                pendingMessage.MarkProcessed(_timeProvider.GetUtcNow().UtcDateTime);
                processedCount++;
            }
            catch (Exception exception)
            {
                pendingMessage.MarkFailed(exception.ToString());
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return processedCount;
    }
}
