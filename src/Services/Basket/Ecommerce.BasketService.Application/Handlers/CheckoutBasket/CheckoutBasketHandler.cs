namespace Ecommerce.BasketService.Application;

public sealed class CheckoutBasketHandler
{
    private readonly IBasketDbContext _dbContext;

    public CheckoutBasketHandler(IBasketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BasketDto> ExecuteAsync(CheckoutBasketCommand command, CancellationToken cancellationToken)
    {
        var basket = await _dbContext.GetBasketByIdAsync(command.BasketId, cancellationToken);
        if (basket is null)
        {
            throw new BasketNotFoundException("The basket was not found.");
        }

        basket.Checkout();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return basket.ToDto();
    }
}

public sealed record CheckoutBasketCommand(Guid BasketId);

