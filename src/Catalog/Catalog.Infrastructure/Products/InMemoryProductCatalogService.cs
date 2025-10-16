namespace Catalog.Infrastructure.Products;

using Catalog.Application.Products;
using Catalog.Domain.Products;

public sealed class InMemoryProductCatalogService : IProductCatalogService
{
    private static readonly IReadOnlyCollection<Product> SeedProducts = new List<Product>
    {
        new(Guid.Parse("2e0c52ae-987f-4bea-96de-23b587f7e9a9"), "Sample Product", "A placeholder catalog item.")
    };

    public IReadOnlyCollection<Product> GetProducts() => SeedProducts;
}
