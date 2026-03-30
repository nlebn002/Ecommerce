using Ecommerce.BasketService.Application;
using Ecommerce.BasketService.Domain;
using Ecommerce.Common.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.BasketService.Api;

public static class AddOrUpdateBasketItemEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder group)
    {
        group.MapPut("/{basketId}/items", HandleAsync);
        return group;
    }

    private static async Task<Ok<BasketDto>> HandleAsync(
        [FromRoute] Guid basketId,
        [FromBody] UpsertBasketItemRequest request,
        IValidator<UpsertBasketItemRequest> validator,
        AddOrUpdateBasketItemHandler handler,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw BasketException.Validation(
                BasketErrorCode.RequestValidationFailed,
                "API validation failed.",
                ValidationErrorDictionary.Create(
                    validationResult.Errors,
                    error => error.PropertyName,
                    error => error.ErrorMessage));
        }

        var basket = await handler.ExecuteAsync(
            new AddOrUpdateBasketItemCommand(basketId, request.ProductId, request.ProductName, request.Quantity, request.UnitPrice),
            cancellationToken);

        return TypedResults.Ok(basket);
    }
}

public sealed record UpsertBasketItemRequest(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);

public sealed class UpsertBasketItemRequestValidator : AbstractValidator<UpsertBasketItemRequest>
{
    public UpsertBasketItemRequestValidator()
    {
        RuleFor(request => request.ProductId).NotEqual(Guid.Empty);
        RuleFor(request => request.ProductName).NotEmpty();
        RuleFor(request => request.Quantity).GreaterThan(0);
        RuleFor(request => request.UnitPrice).GreaterThanOrEqualTo(0);
    }
}

