using Ecommerce.OrderService.Application;
using Ecommerce.Common.Messaging.Outbox;
using Ecommerce.Common.Persistence;
using Ecommerce.OrderService.Infrastructure.Messaging.Consumers;
using Ecommerce.OrderService.Infrastructure.Messaging.IntegrationEvents;
using Ecommerce.OrderService.Infrastructure.Messaging.Outbox;
using Ecommerce.OrderService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.OrderService.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var orderDbConnectionString =
            configuration.GetConnectionString("OrderDb")
            ?? throw new ArgumentNullException("Order db connection string is null");

        var messageBrokerConnectionString =
            configuration.GetConnectionString("MessageBroker")
            ?? throw new ArgumentNullException("Message broker connection string is null");

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<AuditSaveChangesInterceptor>();
        services.AddDbContext<OrderDbContext>((serviceProvider, options) =>
            options.UseNpgsql(orderDbConnectionString)
                .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));
        services.AddScoped<IOrderDbContext>(serviceProvider => serviceProvider.GetRequiredService<OrderDbContext>());
        services.AddScoped<IOutboxDbContext>(serviceProvider => serviceProvider.GetRequiredService<OrderDbContext>());

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
            });

        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();
            bus.AddConsumer<BasketCheckedOutConsumer>();
            bus.AddConsumer<ShipmentRepricedConsumer>();
            bus.AddConsumer<ShipmentFailedConsumer>();
            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(messageBrokerConnectionString));
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddSingleton<OrderIntegrationEventFactory>();
        services.AddScoped<IDomainEventOutboxMessageFactory, DomainEventOutboxMessageFactory>();
        services.AddScoped<IOutboxMessagePublisher, OutboxMessagePublisher>();
        services.AddScoped<OutboxMessageProcessor>();
        services.AddHostedService<OutboxPublisherService>();

        return services;
    }
}
