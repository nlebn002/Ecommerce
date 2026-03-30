namespace Ecommerce.BasketService.Domain;

public interface IEntity : Ecommerce.Common.Persistence.IEntity
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
