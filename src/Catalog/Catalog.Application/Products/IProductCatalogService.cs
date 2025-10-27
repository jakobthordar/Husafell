namespace Catalog.Application.Products;

using Catalog.Domain.Products;

public interface IProductCatalogService
{
    Task<IReadOnlyCollection<Product>> GetProductsAsync(CancellationToken cancellationToken = default);
}
