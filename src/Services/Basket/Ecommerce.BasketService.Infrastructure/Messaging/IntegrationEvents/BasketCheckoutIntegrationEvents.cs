namespace Ecommerce.BasketService.Infrastructure.Messaging.IntegrationEvents;

public sealed record BasketCheckoutIntegrationEvents(
    Ecommerce.Contracts.V1.BasketCheckedOut V1,
    Ecommerce.Contracts.V2.BasketCheckedOut V2);
