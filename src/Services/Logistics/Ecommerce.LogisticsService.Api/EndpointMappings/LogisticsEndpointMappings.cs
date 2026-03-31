using Asp.Versioning;

namespace Ecommerce.LogisticsService.Api;

public static class LogisticsEndpointMappings
{
    public static IEndpointRouteBuilder MapLogisticsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = endpoints.MapGroup("/api/v{version:apiVersion}/shipments")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(new ApiVersion(1, 0));

        GetShipmentEndpoint.Map(group);
        GetShipmentByOrderEndpoint.Map(group);

        return endpoints;
    }
}
