using Ecommerce.BasketService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ecommerce.BasketService.Api;

public static class SeedBasketDevDataEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/internal/dev/seed", HandleAsync)
            .WithTags("Development")
            .WithSummary("Seeds Basket development data")
            .WithDescription("Applies Basket migrations and inserts deterministic sample data in Development only.")
            .ExcludeFromDescription();

        return endpoints;
    }

    private static async Task<Ok<SeedBasketDevDataResponse>> HandleAsync(
        IHostEnvironment environment,
        IServiceProvider services,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var result = await BasketDevDataSeeder.SeedAsync(
            services,
            loggerFactory.CreateLogger(typeof(BasketDevDataSeeder)),
            environment,
            cancellationToken);

        return TypedResults.Ok(new SeedBasketDevDataResponse(result.Applied, result.Message));
    }
}

public sealed record SeedBasketDevDataResponse(bool Applied, string Message);
