using Ecommerce.BasketService.Application;
using Ecommerce.BasketService.Domain;
using Ecommerce.Common.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.BasketService.Api;

public static class CreateBasketEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder group)
    {
        group.MapPost(string.Empty, HandleAsync);
        return group;
    }

    private static async Task<Created<BasketDto>> HandleAsync(
        [FromBody] CreateBasketRequest request,
        IValidator<CreateBasketRequest> validator,
        CreateBasketHandler handler,
        HttpContext httpContext,
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

        var basket = await handler.ExecuteAsync(new CreateBasketCommand(request.CustomerId), cancellationToken);
        var location = $"{httpContext.Request.Path.Value?.TrimEnd('/')}/{basket.BasketId}";

        return TypedResults.Created(location, basket);
    }
}

public sealed record CreateBasketRequest(Guid CustomerId);

public sealed class CreateBasketRequestValidator : AbstractValidator<CreateBasketRequest>
{
    public CreateBasketRequestValidator()
    {
        RuleFor(request => request.CustomerId).NotEqual(Guid.Empty).WithMessage("Customer id is required.");
    }
}
