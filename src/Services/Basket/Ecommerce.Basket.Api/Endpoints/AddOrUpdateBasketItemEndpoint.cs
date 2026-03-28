using Ecommerce.Basket.Application;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Basket.Api;

public static class AddOrUpdateBasketItemEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder group)
    {
        group.MapPut("/{basketId}/items", HandleAsync);
        return group;
    }

    private static async Task<Ok<BasketDto>> HandleAsync(
        [FromRoute] string basketId,
        [FromBody] UpsertBasketItemRequest request,
        IValidator<UpsertBasketItemRequest> validator,
        AddOrUpdateBasketItemService service,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ApiValidationException(validationResult);
        }

        var basket = await service.ExecuteAsync(
            new AddOrUpdateBasketItemCommand(basketId, request.ProductId, request.ProductName, request.Quantity, request.UnitPrice),
            cancellationToken);

        return TypedResults.Ok(basket);
    }
}

public sealed record UpsertBasketItemRequest(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);

public sealed class UpsertBasketItemRequestValidator : AbstractValidator<UpsertBasketItemRequest>
{
    public UpsertBasketItemRequestValidator()
    {
        RuleFor(request => request.ProductId).NotEmpty();
        RuleFor(request => request.ProductName).NotEmpty();
        RuleFor(request => request.Quantity).GreaterThan(0);
        RuleFor(request => request.UnitPrice).GreaterThanOrEqualTo(0);
    }
}
