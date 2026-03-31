using Asp.Versioning;

namespace Ecommerce.BasketService.Api;

public static class BasketEndpointMappings
{
    public static IEndpointRouteBuilder MapBasketEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = endpoints.MapGroup("/api/v{version:apiVersion}/baskets")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(new ApiVersion(1, 0));

        CreateBasketEndpoint.Map(group);
        GetBasketEndpoint.Map(group);
        AddOrUpdateBasketItemEndpoint.Map(group);
        RemoveBasketItemEndpoint.Map(group);
        CheckoutBasketEndpoint.Map(group);

        return endpoints;
    }
}

