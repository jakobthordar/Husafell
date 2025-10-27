namespace Catalog.Application.Works;

using Catalog.Domain.Works;

/// <summary>
/// Defines the contract for retrieving catalogued works.
/// </summary>
public interface IWorkCatalogService
{
    /// <summary>
    /// Retrieves the works currently available in the catalog.
    /// </summary>
    /// <returns>An immutable collection containing the catalogued works.</returns>
    IReadOnlyCollection<Work> GetWorks();
}
