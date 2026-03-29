using Ecommerce.BasketService.Domain;

namespace Ecommerce.BasketService.Application;

public sealed class RemoveBasketItemHandler
{
    private readonly IBasketDbContext _dbContext;

    public RemoveBasketItemHandler(IBasketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BasketDto> ExecuteAsync(RemoveBasketItemCommand command, CancellationToken cancellationToken)
    {
        var basket = await _dbContext.GetBasketByIdAsync(command.BasketId, cancellationToken);
        if (basket is null)
        {
            throw BasketException.NotFound(BasketErrorCode.BasketNotFound, "The basket was not found.");
        }

        basket.RemoveItem(command.ProductId);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return basket.ToDto();
    }
}

public sealed record RemoveBasketItemCommand(Guid BasketId, Guid ProductId);

