using Ecommerce.OrderService.Application;
using Ecommerce.OrderService.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.OrderService.Api;

public static class GetOrderEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder group)
    {
        group.MapGet("/{orderId}", HandleAsync);
        return group;
    }

    private static async Task<Ok<OrderDetailsDto>> HandleAsync(
        [FromRoute] Guid orderId,
        IValidator<GetOrderRequest> validator,
        GetOrderHandler handler,
        CancellationToken cancellationToken)
    {
        var request = new GetOrderRequest(orderId);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw OrderException.Validation(
                OrderErrorCode.RequestValidationFailed,
                "API validation failed.",
                validationResult.Errors
                    .GroupBy(error => error.PropertyName, StringComparer.Ordinal)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray(),
                        StringComparer.Ordinal));
        }

        var order = await handler.ExecuteAsync(new GetOrderQuery(request.OrderId), cancellationToken);
        return TypedResults.Ok(order);
    }
}

public sealed record GetOrderRequest(Guid OrderId);

public sealed class GetOrderRequestValidator : AbstractValidator<GetOrderRequest>
{
    public GetOrderRequestValidator()
    {
        RuleFor(request => request.OrderId).NotEqual(Guid.Empty);
    }
}
