using Ecommerce.BasketService.Domain;

namespace Ecommerce.BasketService.Application;

public sealed class CreateBasketHandler
{
    private readonly IBasketDbContext _dbContext;

    public CreateBasketHandler(IBasketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BasketDto> ExecuteAsync(CreateBasketCommand command, CancellationToken cancellationToken)
    {
        if (command.CustomerId == Guid.Empty)
        {
            throw BasketValidationException.For("customerId", "Customer id is required.");
        }

        var basket = Basket.Create(command.CustomerId);
        await _dbContext.Baskets.AddAsync(basket, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return basket.ToDto();
    }
}

public sealed record CreateBasketCommand(Guid CustomerId);
