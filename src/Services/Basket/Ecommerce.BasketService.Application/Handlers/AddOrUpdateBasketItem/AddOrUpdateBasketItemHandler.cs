using Ecommerce.BasketService.Domain;

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
        var basket = await _dbContext.GetBasketByIdAsync(command.BasketId, cancellationToken);
        if (basket is null)
        {
            throw BasketException.NotFound(BasketErrorCode.BasketNotFound, "The basket was not found.");
        }

        basket.AddOrUpdateItem(command.ProductId, command.ProductName, command.Quantity, command.UnitPrice);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return basket.ToDto();
    }
}

public sealed record AddOrUpdateBasketItemCommand(
    Guid BasketId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);

