using Ecommerce.BasketService.Application;
using Ecommerce.BasketService.Domain;
using Ecommerce.Common.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.BasketService.Api;

public static class CheckoutBasketEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder group)
    {
        group.MapPost("/{basketId}/checkout", HandleAsync);
        return group;
    }

    private static async Task<Ok<BasketDto>> HandleAsync(
        [FromRoute] Guid basketId,
        IValidator<CheckoutBasketRequest> validator,
        CheckoutBasketHandler handler,
        CancellationToken cancellationToken)
    {
        var request = new CheckoutBasketRequest(basketId);
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

        var basket = await handler.ExecuteAsync(new CheckoutBasketCommand(request.BasketId), cancellationToken);
        return TypedResults.Ok(basket);
    }
}

public sealed record CheckoutBasketRequest(Guid BasketId);

public sealed class CheckoutBasketRequestValidator : AbstractValidator<CheckoutBasketRequest>
{
    public CheckoutBasketRequestValidator()
    {
        RuleFor(request => request.BasketId).NotEqual(Guid.Empty).WithMessage("Basket id is required.");
    }
}

