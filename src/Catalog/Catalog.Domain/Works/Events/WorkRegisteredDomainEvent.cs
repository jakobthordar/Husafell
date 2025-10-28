using Catalog.Domain.Abstractions;

namespace Catalog.Domain.Works.Events;

/// <summary>
/// Raised when a new work is registered in the catalog.
/// </summary>
/// <param name="WorkId">The unique identifier for the newly registered work.</param>
/// <param name="AccessionNumber">The accession number assigned to the work.</param>
public sealed record WorkRegisteredDomainEvent(Guid WorkId, string AccessionNumber) : DomainEvent;
