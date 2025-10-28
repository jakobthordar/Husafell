using Catalog.Domain.Abstractions;

namespace Catalog.Domain.Works.Events;

/// <summary>
/// Raised when a new asset is associated with a work.
/// </summary>
/// <param name="WorkId">The unique identifier of the work to which the asset was added.</param>
/// <param name="AssetId">The unique identifier of the asset that was added.</param>
/// <param name="IsPrimary">Indicates whether the asset became the primary asset for the work.</param>
public sealed record AssetAddedToWorkDomainEvent(Guid WorkId, Guid AssetId, bool IsPrimary) : DomainEvent;
