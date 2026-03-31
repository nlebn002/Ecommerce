using Ecommerce.LogisticsService.Application;
using FluentValidation;

namespace Ecommerce.LogisticsService.Api;

public static class GetShipmentEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/{shipmentId:guid}", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        Guid shipmentId,
        IValidator<GetShipmentRequest> validator,
        GetShipmentHandler handler,
        CancellationToken cancellationToken)
    {
        var request = new GetShipmentRequest(shipmentId);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var shipment = await handler.ExecuteAsync(new GetShipmentQuery(request.ShipmentId), cancellationToken);
        return Results.Ok(shipment);
    }
}

public sealed record GetShipmentRequest(Guid ShipmentId);

public sealed class GetShipmentRequestValidator : AbstractValidator<GetShipmentRequest>
{
    public GetShipmentRequestValidator()
    {
        RuleFor(request => request.ShipmentId).NotEmpty();
    }
}
