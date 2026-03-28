using Ecommerce.Basket.Application;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Basket.Api;

public static class GetBasketEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder group)
    {
        group.MapGet("/{basketId}", HandleAsync);
        return group;
    }

    private static async Task<Ok<BasketDto>> HandleAsync(
        [FromRoute] string basketId,
        [FromQuery] string? customerId,
        IValidator<GetBasketRequest> validator,
        GetBasketService service,
        CancellationToken cancellationToken)
    {
        var request = new GetBasketRequest(basketId, customerId);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ApiValidationException(validationResult);
        }

        var basket = await service.ExecuteAsync(new GetBasketQuery(request.BasketId, request.CustomerId), cancellationToken);
        return TypedResults.Ok(basket);
    }
}

public sealed record GetBasketRequest(string BasketId, string? CustomerId);

public sealed class GetBasketRequestValidator : AbstractValidator<GetBasketRequest>
{
    public GetBasketRequestValidator()
    {
        RuleFor(request => request.BasketId).NotEmpty();
    }
}
