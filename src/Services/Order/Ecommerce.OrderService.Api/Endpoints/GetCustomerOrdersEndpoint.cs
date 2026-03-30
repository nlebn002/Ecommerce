using Ecommerce.OrderService.Application;
using Ecommerce.Common.Validation;
using Ecommerce.OrderService.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.OrderService.Api;

public static class GetCustomerOrdersEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder group)
    {
        group.MapGet("/by-customer/{customerId}", HandleAsync);
        return group;
    }

    private static async Task<Ok<IReadOnlyList<OrderSummaryDto>>> HandleAsync(
        [FromRoute] Guid customerId,
        IValidator<GetCustomerOrdersRequest> validator,
        GetCustomerOrdersHandler handler,
        CancellationToken cancellationToken)
    {
        var request = new GetCustomerOrdersRequest(customerId);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw OrderException.Validation(
                OrderErrorCode.RequestValidationFailed,
                "API validation failed.",
                ValidationErrorDictionary.Create(
                    validationResult.Errors,
                    error => error.PropertyName,
                    error => error.ErrorMessage));
        }

        var orders = await handler.ExecuteAsync(new GetCustomerOrdersQuery(request.CustomerId), cancellationToken);
        return TypedResults.Ok(orders);
    }
}

public sealed record GetCustomerOrdersRequest(Guid CustomerId);

public sealed class GetCustomerOrdersRequestValidator : AbstractValidator<GetCustomerOrdersRequest>
{
    public GetCustomerOrdersRequestValidator()
    {
        RuleFor(request => request.CustomerId).NotEqual(Guid.Empty);
    }
}
