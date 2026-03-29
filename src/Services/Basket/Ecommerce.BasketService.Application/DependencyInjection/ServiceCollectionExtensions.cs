using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.BasketService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBasketApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateBasketHandler>();
        services.AddScoped<GetBasketHandler>();
        services.AddScoped<AddOrUpdateBasketItemHandler>();
        services.AddScoped<RemoveBasketItemHandler>();
        services.AddScoped<CheckoutBasketHandler>();
        return services;
    }
}

