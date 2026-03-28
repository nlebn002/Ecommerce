namespace Ecommerce.BasketService.Domain;

public interface IEntity
{
    Guid Id { get; }

    DateTime CreatedDate { get; }

    DateTime? UpdatedDate { get; }

    bool IsDeleted { get; }
}
