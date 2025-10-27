using Catalog.Domain.Abstractions;

namespace Catalog.Domain.Works.Events;

/// <summary>
/// Raised when the description of a work changes.
/// </summary>
/// <param name="WorkId">The unique identifier of the work whose description changed.</param>
public sealed record WorkDescriptionUpdatedDomainEvent(Guid WorkId) : DomainEvent;
