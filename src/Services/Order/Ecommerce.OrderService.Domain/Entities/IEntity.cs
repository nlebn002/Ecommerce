namespace Ecommerce.OrderService.Domain;

public interface IEntity
{
    Guid Id { get; }

    Guid ConcurrencyToken { get; }

    DateTime CreatedDate { get; }

    DateTime? UpdatedDate { get; }

    bool IsDeleted { get; }
}
