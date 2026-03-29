using Ecommerce.BasketService.Application;
using Ecommerce.BasketService.Infrastructure.Persistence;
using Ecommerce.BasketService.Infrastructure.Persistence.Interceptors;
using Ecommerce.BasketService.Infrastructure.Messaging.IntegrationEvents;
using Ecommerce.BasketService.Infrastructure.Messaging.Outbox;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.BasketService.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBasketInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var basketDbConnectionString =
            configuration.GetConnectionString("BasketDb")
            ?? throw new ArgumentNullException("Basket db connection string is null");

        var messageBrokerConnectionString =
            configuration.GetConnectionString("MessageBroker")
            ?? throw new ArgumentNullException("Message broker connection string is null");

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<AuditSaveChangesInterceptor>();
        services.AddDbContext<BasketDbContext>((serviceProvider, options) =>
            options.UseNpgsql(basketDbConnectionString)
                .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));
        services.AddScoped<IBasketDbContext>(serviceProvider => serviceProvider.GetRequiredService<BasketDbContext>());
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
            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(messageBrokerConnectionString));
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddSingleton<BasketCheckoutIntegrationEventFactory>();
        services.AddScoped<IDomainEventOutboxMessageFactory, DomainEventOutboxMessageFactory>();
        services.AddScoped<IOutboxMessagePublisher, OutboxMessagePublisher>();
        services.AddScoped<OutboxMessageProcessor>();
        services.AddHostedService<OutboxPublisherService>();

        return services;
    }
}

