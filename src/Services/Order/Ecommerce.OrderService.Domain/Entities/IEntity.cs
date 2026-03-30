namespace Ecommerce.OrderService.Domain;

public interface IEntity : Ecommerce.Common.Persistence.IEntity
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
}
