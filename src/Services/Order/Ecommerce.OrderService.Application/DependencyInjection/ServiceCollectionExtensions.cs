using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.OrderService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderApplication(this IServiceCollection services)
    {
        services.AddScoped<GetOrderHandler>();
        services.AddScoped<GetCustomerOrdersHandler>();
        services.AddScoped<CreateOrderFromBasketCheckoutHandler>();
        services.AddScoped<ConfirmOrderHandler>();
        services.AddScoped<CancelOrderHandler>();
        return services;
    }
}
