using Ecommerce.BasketService.Domain;

namespace Ecommerce.BasketService.Application;

public sealed class GetBasketHandler
{
    private readonly IBasketDbContext _dbContext;

    public GetBasketHandler(IBasketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BasketDto> ExecuteAsync(GetBasketQuery query, CancellationToken cancellationToken)
    {
        var basket = await _dbContext.GetBasketByIdAsync(query.BasketId, cancellationToken);
        if (basket is null)
        {
            throw BasketException.NotFound(BasketErrorCode.BasketNotFound, "The basket was not found.");
        }

        return basket.ToDto();
    }
}

public sealed record GetBasketQuery(Guid BasketId);

