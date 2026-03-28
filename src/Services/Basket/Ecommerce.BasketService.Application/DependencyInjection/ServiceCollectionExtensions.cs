using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.BasketService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBasketApplication(this IServiceCollection services)
    {
        services.AddScoped<GetBasketService>();
        services.AddScoped<AddOrUpdateBasketItemService>();
        services.AddScoped<RemoveBasketItemService>();
        services.AddScoped<CheckoutBasketService>();
        return services;
    }
}

