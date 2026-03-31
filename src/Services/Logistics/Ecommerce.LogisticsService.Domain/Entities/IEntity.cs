namespace Ecommerce.LogisticsService.Domain;

public interface IEntity : Ecommerce.Common.Persistence.IEntity
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
}
