using Catalog.Domain.Abstractions;

namespace Catalog.Domain.Works.Events;

/// <summary>
/// Raised when an asset is removed from a work.
/// </summary>
/// <param name="WorkId">The unique identifier of the work from which the asset was removed.</param>
/// <param name="AssetId">The unique identifier of the asset that was removed.</param>
/// <param name="WasPrimary">Indicates whether the asset was the primary representation prior to removal.</param>
public sealed record AssetRemovedFromWorkDomainEvent(Guid WorkId, Guid AssetId, bool WasPrimary) : DomainEvent;
