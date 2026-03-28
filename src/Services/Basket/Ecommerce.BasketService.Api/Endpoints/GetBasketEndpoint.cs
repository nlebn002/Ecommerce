using Ecommerce.BasketService.Api.Exceptions;
using Ecommerce.BasketService.Application;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.BasketService.Api;

public static class GetBasketEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder group)
    {
        group.MapGet("/{basketId}", HandleAsync);
        return group;
    }

    private static async Task<Ok<BasketDto>> HandleAsync(
        [FromRoute] Guid basketId,
        [FromQuery] Guid? customerId,
        IValidator<GetBasketRequest> validator,
        GetBasketHandler handler,
        CancellationToken cancellationToken)
    {
        var request = new GetBasketRequest(basketId, customerId);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ApiValidationException(validationResult);
        }

        var basket = await handler.ExecuteAsync(new GetBasketQuery(request.BasketId, request.CustomerId), cancellationToken);
        return TypedResults.Ok(basket);
    }
}

public sealed record GetBasketRequest(Guid BasketId, Guid? CustomerId);

public sealed class GetBasketRequestValidator : AbstractValidator<GetBasketRequest>
{
    public GetBasketRequestValidator()
    {
        RuleFor(request => request.BasketId).NotEqual(Guid.Empty);
    }
}

