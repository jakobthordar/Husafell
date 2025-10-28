namespace Catalog.Domain.Abstractions;

/// <summary>
/// Defines the contract for a domain event.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the timestamp indicating when the domain event occurred.
    /// </summary>
    DateTimeOffset OccurredOn { get; }
}

/// <summary>
/// Serves as the base record for domain events.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
