namespace Ecommerce.OrderService.Domain;

public abstract class Entity : IEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Guid ConcurrencyToken { get; protected set; }

    public DateTime CreatedDate { get; protected set; }

    public DateTime? UpdatedDate { get; protected set; }

    public bool IsDeleted { get; protected set; }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
