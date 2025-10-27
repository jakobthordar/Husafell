using Catalog.Domain.Abstractions;

namespace Catalog.Domain.Works.Events;

/// <summary>
/// Raised when the title or slug of a work changes.
/// </summary>
/// <param name="WorkId">The unique identifier of the work that was renamed.</param>
/// <param name="Slug">The slug associated with the work after the rename.</param>
public sealed record WorkRenamedDomainEvent(Guid WorkId, string Slug) : DomainEvent;
