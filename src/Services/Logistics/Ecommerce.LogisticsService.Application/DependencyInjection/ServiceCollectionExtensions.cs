using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.LogisticsService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLogisticsApplication(this IServiceCollection services)
    {
        services.AddScoped<GetShipmentHandler>();
        services.AddScoped<GetShipmentByOrderHandler>();
        services.AddScoped<ReserveShipmentForOrderHandler>();
        services.AddScoped<FailShipmentHandler>();
        return services;
    }
}
