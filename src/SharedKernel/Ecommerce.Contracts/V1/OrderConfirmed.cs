namespace Ecommerce.Contracts.V1;

public sealed record OrderConfirmed : IntegrationEvent
{
    public required string OrderId { get; init; }

    public decimal ShippingPrice { get; init; }

    public decimal FinalTotal { get; init; }

    public required string Status { get; init; }
}
