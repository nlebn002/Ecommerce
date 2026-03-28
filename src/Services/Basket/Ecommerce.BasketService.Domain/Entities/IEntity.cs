namespace Ecommerce.BasketService.Domain;

public interface IEntity
{
    Guid Id { get; }

    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    DateTime CreatedDate { get; }

    DateTime? UpdatedDate { get; }

    bool IsDeleted { get; }

    void ClearDomainEvents();
}
