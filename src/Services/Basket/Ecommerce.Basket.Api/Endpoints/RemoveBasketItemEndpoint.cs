using Ecommerce.Basket.Application;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Basket.Api;

public static class RemoveBasketItemEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder group)
    {
        group.MapDelete("/{basketId}/items/{productId}", HandleAsync);
        return group;
    }

    private static async Task<Ok<BasketDto>> HandleAsync(
        [FromRoute] string basketId,
        [FromRoute] string productId,
        IValidator<RemoveBasketItemRequest> validator,
        RemoveBasketItemService service,
        CancellationToken cancellationToken)
    {
        var request = new RemoveBasketItemRequest(basketId, productId);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ApiValidationException(validationResult);
        }

        var basket = await service.ExecuteAsync(new RemoveBasketItemCommand(request.BasketId, request.ProductId), cancellationToken);
        return TypedResults.Ok(basket);
    }
}

public sealed record RemoveBasketItemRequest(string BasketId, string ProductId);

public sealed class RemoveBasketItemRequestValidator : AbstractValidator<RemoveBasketItemRequest>
{
    public RemoveBasketItemRequestValidator()
    {
        RuleFor(request => request.BasketId).NotEmpty().WithMessage("Basket id is required.");
        RuleFor(request => request.ProductId).NotEmpty().WithMessage("Product id is required.");
    }
}
