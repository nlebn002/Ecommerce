namespace Ecommerce.BasketService.Application;

public sealed class AddOrUpdateBasketItemHandler
{
    private readonly IBasketDbContext _dbContext;

    public AddOrUpdateBasketItemHandler(IBasketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BasketDto> ExecuteAsync(AddOrUpdateBasketItemCommand command, CancellationToken cancellationToken)
    {
        if (command.Quantity < 1)
        {
            throw BasketValidationException.For("quantity", "Quantity must be greater than zero.");
        }

        var basket = await _dbContext.GetBasketAggregateByIdAsync(command.BasketId, cancellationToken);
        if (basket is null)
        {
            throw new BasketNotFoundException("The basket was not found.");
        }

        if (!basket.IsActive)
        {
            throw new BasketConflictException("Checked out baskets cannot be changed.");
        }

        basket.AddOrUpdateItem(command.ProductId, command.ProductName, command.Quantity, command.UnitPrice);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return basket.ToDto();
    }
}

public sealed record AddOrUpdateBasketItemCommand(
    Guid BasketId,
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);

