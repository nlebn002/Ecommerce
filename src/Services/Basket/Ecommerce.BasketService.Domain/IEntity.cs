namespace Ecommerce.BasketService.Domain;

public interface IEntity
{
    Guid Id { get; }

    DateTime CreatedDate { get; }

    bool IsDeleted { get; }
}
