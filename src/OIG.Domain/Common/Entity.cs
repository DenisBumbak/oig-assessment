namespace OIG.Domain.Common;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; }

    private readonly List<object> _domainEvents = new();
    public IReadOnlyCollection<object> DomainEvents => _domainEvents;

    protected Entity(TId id) => Id = id;

    protected void AddDomainEvent(object @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
