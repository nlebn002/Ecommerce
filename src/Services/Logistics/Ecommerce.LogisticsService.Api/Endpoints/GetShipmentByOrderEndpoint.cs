using Ecommerce.LogisticsService.Application;
using FluentValidation;

namespace Ecommerce.LogisticsService.Api;

public static class GetShipmentByOrderEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/by-order/{orderId:guid}", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        Guid orderId,
        IValidator<GetShipmentByOrderRequest> validator,
        GetShipmentByOrderHandler handler,
        CancellationToken cancellationToken)
    {
        var request = new GetShipmentByOrderRequest(orderId);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var shipment = await handler.ExecuteAsync(new GetShipmentByOrderQuery(request.OrderId), cancellationToken);
        return Results.Ok(shipment);
    }
}

public sealed record GetShipmentByOrderRequest(Guid OrderId);

public sealed class GetShipmentByOrderRequestValidator : AbstractValidator<GetShipmentByOrderRequest>
{
    public GetShipmentByOrderRequestValidator()
    {
        RuleFor(request => request.OrderId).NotEmpty();
    }
}
