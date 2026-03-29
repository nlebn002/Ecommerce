using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce.BasketService.Infrastructure.Messaging.Outbox;

public sealed class OutboxPublisherService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IOptions<OutboxProcessorOptions> _options;
    private readonly ILogger<OutboxPublisherService> _logger;

    public OutboxPublisherService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<OutboxProcessorOptions> options,
        ILogger<OutboxPublisherService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<OutboxMessageProcessor>();
                var processedCount = await processor.ProcessPendingMessagesAsync(_options.Value.BatchSize, stoppingToken);

                if (processedCount > 0)
                {
                    continue;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to process Basket outbox messages.");
            }

            await Task.Delay(_options.Value.PollInterval, stoppingToken);
        }
    }
}
