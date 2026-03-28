using Ecommerce.BasketService.Application;
using Ecommerce.BasketService.Infrastructure.Messaging;
using Ecommerce.BasketService.Infrastructure.Persistence;
using Ecommerce.BasketService.Infrastructure.Persistence.Interceptors;
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

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<AuditSaveChangesInterceptor>();
        services.AddDbContext<BasketDbContext>((serviceProvider, options) =>
            options.UseNpgsql(basketDbConnectionString)
                .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));
        services.AddScoped<IBasketDbContext>(serviceProvider => serviceProvider.GetRequiredService<BasketDbContext>());

        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();
            bus.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["MessageBroker:Host"] ?? "localhost";
                cfg.Host(host, "/", hostConfiguration =>
                {
                    hostConfiguration.Username("guest");
                    hostConfiguration.Password("guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IBasketCheckoutPublisher, BasketCheckoutPublisher>();

        return services;
    }
}

