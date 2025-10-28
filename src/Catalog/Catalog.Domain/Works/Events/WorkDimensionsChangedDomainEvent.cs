using Catalog.Domain.Abstractions;
using Catalog.Domain.Works.ValueObjects;

namespace Catalog.Domain.Works.Events;

/// <summary>
/// Raised when the physical dimensions associated with a work are updated.
/// </summary>
/// <param name="WorkId">The unique identifier of the work that was updated.</param>
/// <param name="Dimensions">The dimensions that were applied to the work.</param>
public sealed record WorkDimensionsChangedDomainEvent(Guid WorkId, Dimensions? Dimensions) : DomainEvent;
