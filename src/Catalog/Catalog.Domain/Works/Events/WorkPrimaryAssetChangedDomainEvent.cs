using Catalog.Domain.Abstractions;

namespace Catalog.Domain.Works.Events;

/// <summary>
/// Raised when the primary asset for a work changes.
/// </summary>
/// <param name="WorkId">The unique identifier of the work affected by the change.</param>
/// <param name="PrimaryAssetId">The identifier of the asset that is now primary.</param>
public sealed record WorkPrimaryAssetChangedDomainEvent(Guid WorkId, Guid PrimaryAssetId) : DomainEvent;
