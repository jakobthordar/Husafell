namespace Catalog.Application.Products;

using Catalog.Domain.Products;

public interface IProductCatalogService
{
    IReadOnlyCollection<Product> GetProducts();
}
