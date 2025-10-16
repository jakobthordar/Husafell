using Catalog.Infrastructure.Products;

namespace Catalog.Infrastructure.Tests;

public class InMemoryProductCatalogServiceTests
{
    [Fact]
    public void GetProducts_ShouldReturnSeededProducts()
    {
        var service = new InMemoryProductCatalogService();

        var products = service.GetProducts();

        Assert.NotEmpty(products);
    }
}
