using Asp.Versioning;

namespace Ecommerce.OrderService.Api;

public static class OrderEndpointMappings
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = endpoints.MapGroup("/api/v{version:apiVersion}/orders")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(new ApiVersion(1, 0));

        GetOrderEndpoint.Map(group);
        GetCustomerOrdersEndpoint.Map(group);

        return endpoints;
    }
}
