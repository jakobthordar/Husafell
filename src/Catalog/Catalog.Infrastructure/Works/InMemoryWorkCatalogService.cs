namespace Catalog.Infrastructure.Works;

using Catalog.Application.Works;
using Catalog.Domain.Works;
using Catalog.Domain.Works.ValueObjects;

/// <summary>
/// Provides an in-memory implementation of the <see cref="IWorkCatalogService"/> for development and testing scenarios.
/// </summary>
public sealed class InMemoryWorkCatalogService : IWorkCatalogService
{
    private static readonly IReadOnlyCollection<Work> SeedWorks = CreateSeedWorks();

    /// <inheritdoc />
    public IReadOnlyCollection<Work> GetWorks() => SeedWorks;

    private static IReadOnlyCollection<Work> CreateSeedWorks()
    {
        var work = Work.Register(
            id: Guid.Parse("2e0c52ae-987f-4bea-96de-23b587f7e9a9"),
            accessionNumber: AccessionNumber.Create("INV-0001"),
            title: LocalizedText.Create("en", "Sample Work"),
            slug: Slug.Create("sample work"),
            description: LocalizedText.Create("en", "A placeholder catalog entry for demonstration purposes."),
            dimensions: Dimensions.Create(25.5m, 40.2m, null, MeasurementUnit.Centimetres));

        work.AddAsset(
            assetId: Guid.Parse("5ca3b6f8-9e8f-4f2d-a9aa-045ff42dc545"),
            fileName: "sample-work.jpg",
            caption: LocalizedText.Create("en", "Front view"),
            isPrimary: true);

        work.ClearDomainEvents();

        return new List<Work> { work };
    }
}
