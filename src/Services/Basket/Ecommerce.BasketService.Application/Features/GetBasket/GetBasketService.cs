using Ecommerce.BasketService.Domain;

namespace Ecommerce.BasketService.Application;

public sealed class GetBasketService
{
    private readonly IBasketDbContext _dbContext;

    public GetBasketService(IBasketDbContext dbContext)
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

        if (string.IsNullOrWhiteSpace(query.CustomerId))
        {
            throw BasketValidationException.For("customerId", "A customer id is required when creating a basket.");
        }

        basket = Basket.Create(query.BasketId, query.CustomerId);
        await _dbContext.Baskets.AddAsync(basket, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return basket.ToDto();
    }
}

public sealed record GetBasketQuery(string BasketId, string? CustomerId);

