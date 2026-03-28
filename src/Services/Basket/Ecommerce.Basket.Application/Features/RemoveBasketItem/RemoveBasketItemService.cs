namespace Ecommerce.Basket.Application;

public sealed class RemoveBasketItemService
{
    private readonly IBasketDbContext _dbContext;

    public RemoveBasketItemService(IBasketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BasketDto> ExecuteAsync(RemoveBasketItemCommand command, CancellationToken cancellationToken)
    {
        var basket = await _dbContext.GetBasketByIdAsync(command.BasketId, cancellationToken);
        if (basket is null)
        {
            throw new BasketNotFoundException("The basket was not found.");
        }

        if (!basket.IsActive)
        {
            throw new BasketConflictException("Checked out baskets cannot be changed.");
        }

        if (!basket.RemoveItem(command.ProductId))
        {
            throw BasketValidationException.For("productId", "The requested basket item was not found.");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return basket.ToDto();
    }
}

public sealed record RemoveBasketItemCommand(string BasketId, string ProductId);
