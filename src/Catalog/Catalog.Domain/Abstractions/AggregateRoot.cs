namespace Catalog.Domain.Abstractions;

/// <summary>
/// Represents the root entity of an aggregate and exposes domain event management.
/// </summary>
/// <typeparam name="TIdentifier">The identifier type for the aggregate root.</typeparam>
public abstract class AggregateRoot<TIdentifier> : Entity<TIdentifier>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TIdentifier}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the aggregate root.</param>
    protected AggregateRoot(TIdentifier id)
        : base(id)
    {
    }

    /// <summary>
    /// Gets the domain events that have been raised by the aggregate root.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Clears all domain events that have been raised by the aggregate root.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Registers a domain event for later publication.
    /// </summary>
    /// <param name="domainEvent">The domain event to register.</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _domainEvents.Add(domainEvent);
    }
}
