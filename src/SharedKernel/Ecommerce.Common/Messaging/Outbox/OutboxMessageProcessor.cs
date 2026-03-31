using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace Ecommerce.Common.Messaging.Outbox;

public sealed class OutboxMessageProcessor
{
    private readonly IOutboxDbContext _dbContext;
    private readonly IOutboxMessagePublisher _outboxMessagePublisher;
    private readonly OutboxProcessorOptions _options;
    private readonly TimeProvider _timeProvider;

    public OutboxMessageProcessor(
        IOutboxDbContext dbContext,
        IOutboxMessagePublisher outboxMessagePublisher,
        IOptions<OutboxProcessorOptions> options,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _outboxMessagePublisher = outboxMessagePublisher;
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public async Task<int> ProcessPendingMessagesAsync(int batchSize, CancellationToken cancellationToken)
    {
        await using var claimTransaction = await BeginClaimTransactionAsync(cancellationToken);
        var pendingMessages = await LoadPendingMessagesAsync(batchSize, cancellationToken);

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
                pendingMessage.MarkFailed(
                    exception.ToString(),
                    _options.MaxRetryAttempts,
                    _timeProvider.GetUtcNow().UtcDateTime);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        if (claimTransaction is not null)
        {
            await claimTransaction.CommitAsync(cancellationToken);
        }

        return processedCount;
    }

    private Task<IDbContextTransaction?> BeginClaimTransactionAsync(CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational())
        {
            return Task.FromResult<IDbContextTransaction?>(null);
        }

        return BeginRelationalClaimTransactionAsync(cancellationToken);
    }

    private async Task<IDbContextTransaction?> BeginRelationalClaimTransactionAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    private Task<List<OutboxMessage>> LoadPendingMessagesAsync(int batchSize, CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational())
        {
            return _dbContext.OutboxMessages
                .Where(message =>
                    message.ProcessedOnUtc == null &&
                    message.DiscardedOnUtc == null &&
                    message.AttemptCount < _options.MaxRetryAttempts)
                .OrderBy(message => message.OccurredOnUtc)
                .ThenBy(message => message.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }

        return _dbContext.OutboxMessages
            .FromSqlInterpolated($"""
                SELECT "Id", "Type", "Payload", "OccurredOnUtc", "ProcessedOnUtc", "DiscardedOnUtc", "LastError", "AttemptCount"
                FROM "OutboxMessages"
                WHERE "ProcessedOnUtc" IS NULL
                  AND "DiscardedOnUtc" IS NULL
                  AND "AttemptCount" < {_options.MaxRetryAttempts}
                ORDER BY "OccurredOnUtc", "Id"
                LIMIT {batchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);
    }
}
