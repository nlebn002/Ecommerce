using Ecommerce.Basket.Application;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Basket.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBasketInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var basketDbConnectionString =
            configuration.GetConnectionString("BasketDb")
            ?? "Host=localhost;Port=5432;Database=basketdb;Username=ecommerce;Password=ecommerce";

        services.AddDbContext<BasketDbContext>(options => options.UseNpgsql(basketDbConnectionString));
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
