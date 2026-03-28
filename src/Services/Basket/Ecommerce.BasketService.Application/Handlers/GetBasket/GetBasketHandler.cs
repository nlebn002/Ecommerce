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
        if (basket is not null)
        {
            return basket.ToDto();
        }

        if (query.CustomerId is null || query.CustomerId == Guid.Empty)
        {
            throw BasketValidationException.For("customerId", "A customer id is required when creating a basket.");
        }

        basket = Basket.Create(query.CustomerId.Value);
        await _dbContext.Baskets.AddAsync(basket, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return basket.ToDto();
    }
}

public sealed record GetBasketQuery(Guid BasketId, Guid? CustomerId);

