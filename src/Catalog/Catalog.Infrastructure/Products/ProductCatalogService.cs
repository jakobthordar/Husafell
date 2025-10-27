namespace Catalog.Infrastructure.Products;

using Catalog.Application.Products;
using Catalog.Domain.Products;

public sealed class ProductCatalogService : IProductCatalogService
{
    private readonly IProductRepository _repository;

    public ProductCatalogService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _repository.ListAsync(cancellationToken);

        return products;
    }
}
