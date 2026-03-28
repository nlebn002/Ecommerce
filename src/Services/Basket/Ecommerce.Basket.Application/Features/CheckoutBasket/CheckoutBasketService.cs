namespace Ecommerce.Basket.Application;

public sealed class CheckoutBasketService
{
    private readonly IBasketDbContext _dbContext;
    private readonly IBasketCheckoutPublisher _basketCheckoutPublisher;

    public CheckoutBasketService(IBasketDbContext dbContext, IBasketCheckoutPublisher basketCheckoutPublisher)
    {
        _dbContext = dbContext;
        _basketCheckoutPublisher = basketCheckoutPublisher;
    }

    public async Task<BasketDto> ExecuteAsync(CheckoutBasketCommand command, CancellationToken cancellationToken)
    {
        var basket = await _dbContext.GetBasketByIdAsync(command.BasketId, cancellationToken);
        if (basket is null)
        {
            throw new BasketNotFoundException("The basket was not found.");
        }

        if (!basket.IsActive)
        {
            throw new BasketConflictException("The basket has already been checked out.");
        }

        if (basket.Items.Count == 0)
        {
            throw BasketValidationException.For("basketId", "The basket must contain at least one item before checkout.");
        }

        basket.Checkout();
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _basketCheckoutPublisher.PublishAsync(basket, cancellationToken);

        return basket.ToDto();
    }
}

public sealed record CheckoutBasketCommand(string BasketId);
