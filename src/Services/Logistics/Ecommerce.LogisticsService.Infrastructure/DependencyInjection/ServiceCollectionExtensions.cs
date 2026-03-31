using Ecommerce.Common.Messaging.Outbox;
using Ecommerce.Common.Persistence;
using Ecommerce.LogisticsService.Application;
using Ecommerce.LogisticsService.Infrastructure.Messaging.Consumers;
using Ecommerce.LogisticsService.Infrastructure.Messaging.IntegrationEvents;
using Ecommerce.LogisticsService.Infrastructure.Messaging.Outbox;
using Ecommerce.LogisticsService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.LogisticsService.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLogisticsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var logisticsDbConnectionString =
            configuration.GetConnectionString("LogisticsDb")
            ?? throw new ArgumentNullException("Logistics db connection string is null");

        var messageBrokerConnectionString =
            configuration.GetConnectionString("MessageBroker")
            ?? throw new ArgumentNullException("Message broker connection string is null");

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<AuditSaveChangesInterceptor>();
        services.AddDbContext<LogisticsDbContext>((serviceProvider, options) =>
            options.UseNpgsql(logisticsDbConnectionString)
                .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));
        services.AddScoped<ILogisticsDbContext>(serviceProvider => serviceProvider.GetRequiredService<LogisticsDbContext>());
        services.AddScoped<IOutboxDbContext>(serviceProvider => serviceProvider.GetRequiredService<LogisticsDbContext>());

        var outboxSection = configuration.GetSection(OutboxProcessorOptions.SectionName);
        services.AddOptions<OutboxProcessorOptions>()
            .Configure(options =>
            {
                if (int.TryParse(outboxSection["BatchSize"], out var batchSize) && batchSize > 0)
                {
                    options.BatchSize = batchSize;
                }

                if (TimeSpan.TryParse(outboxSection["PollInterval"], out var pollInterval) && pollInterval > TimeSpan.Zero)
                {
                    options.PollInterval = pollInterval;
                }

                if (int.TryParse(outboxSection["MaxRetryAttempts"], out var maxRetryAttempts) && maxRetryAttempts > 0)
                {
                    options.MaxRetryAttempts = maxRetryAttempts;
                }
            });

        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();
            bus.AddConsumer<OrderCreatedConsumer>();
            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(messageBrokerConnectionString));
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddSingleton<LogisticsIntegrationEventFactory>();
        services.AddScoped<IDomainEventOutboxMessageFactory, DomainEventOutboxMessageFactory>();
        services.AddScoped<IOutboxMessagePublisher, OutboxMessagePublisher>();
        services.AddScoped<OutboxMessageProcessor>();
        services.AddHostedService<OutboxPublisherService>();

        return services;
    }
}
