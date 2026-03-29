using Ecommerce.BasketService.Application;
using Ecommerce.BasketService.Domain;
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
        IValidator<GetBasketRequest> validator,
        GetBasketHandler handler,
        CancellationToken cancellationToken)
    {
        var request = new GetBasketRequest(basketId);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw BasketException.Validation(
                BasketErrorCode.RequestValidationFailed,
                "API validation failed.",
                validationResult.Errors
                    .GroupBy(error => error.PropertyName, StringComparer.Ordinal)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray(),
                        StringComparer.Ordinal));
        }

        var basket = await handler.ExecuteAsync(new GetBasketQuery(request.BasketId), cancellationToken);
        return TypedResults.Ok(basket);
    }
}

public sealed record GetBasketRequest(Guid BasketId);

public sealed class GetBasketRequestValidator : AbstractValidator<GetBasketRequest>
{
    public GetBasketRequestValidator()
    {
        RuleFor(request => request.BasketId).NotEqual(Guid.Empty);
    }
}

