using Ecommerce.BasketService.Api.Exceptions;
using Ecommerce.BasketService.Application;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.BasketService.Api;

public static class RemoveBasketItemEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder group)
    {
        group.MapDelete("/{basketId}/items/{productId}", HandleAsync);
        return group;
    }

    private static async Task<Ok<BasketDto>> HandleAsync(
        [FromRoute] Guid basketId,
        [FromRoute] Guid productId,
        IValidator<RemoveBasketItemRequest> validator,
        RemoveBasketItemHandler handler,
        CancellationToken cancellationToken)
    {
        var request = new RemoveBasketItemRequest(basketId, productId);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ApiValidationException(validationResult);
        }

        var basket = await handler.ExecuteAsync(new RemoveBasketItemCommand(request.BasketId, request.ProductId), cancellationToken);
        return TypedResults.Ok(basket);
    }
}

public sealed record RemoveBasketItemRequest(Guid BasketId, Guid ProductId);

public sealed class RemoveBasketItemRequestValidator : AbstractValidator<RemoveBasketItemRequest>
{
    public RemoveBasketItemRequestValidator()
    {
        RuleFor(request => request.BasketId).NotEqual(Guid.Empty).WithMessage("Basket id is required.");
        RuleFor(request => request.ProductId).NotEqual(Guid.Empty).WithMessage("Product id is required.");
    }
}

