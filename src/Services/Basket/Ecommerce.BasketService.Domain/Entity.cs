namespace Ecommerce.BasketService.Domain;

public abstract class Entity : IEntity
{
    public Guid Id { get; protected set; }

    public Guid ConcurrencyToken { get; protected set; }

    public DateTime CreatedDate { get; protected set; }

    public DateTime? UpdatedDate { get; protected set; }

    public bool IsDeleted { get; protected set; }
}
