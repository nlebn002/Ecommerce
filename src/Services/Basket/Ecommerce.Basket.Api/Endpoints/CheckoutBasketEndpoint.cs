using Ecommerce.Basket.Application;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Basket.Api;

public static class CheckoutBasketEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder group)
    {
        group.MapPost("/{basketId}/checkout", HandleAsync);
        return group;
    }

    private static async Task<Ok<BasketDto>> HandleAsync(
        [FromRoute] string basketId,
        IValidator<CheckoutBasketRequest> validator,
        CheckoutBasketService service,
        CancellationToken cancellationToken)
    {
        var request = new CheckoutBasketRequest(basketId);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ApiValidationException(validationResult);
        }

        var basket = await service.ExecuteAsync(new CheckoutBasketCommand(request.BasketId), cancellationToken);
        return TypedResults.Ok(basket);
    }
}

public sealed record CheckoutBasketRequest(string BasketId);

public sealed class CheckoutBasketRequestValidator : AbstractValidator<CheckoutBasketRequest>
{
    public CheckoutBasketRequestValidator()
    {
        RuleFor(request => request.BasketId).NotEmpty().WithMessage("Basket id is required.");
    }
}
