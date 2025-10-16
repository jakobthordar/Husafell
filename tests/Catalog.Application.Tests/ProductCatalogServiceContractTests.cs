using Catalog.Application.Products;

namespace Catalog.Application.Tests;

public class ProductCatalogServiceContractTests
{
    [Fact]
    public void ServiceContract_ShouldBeAbstractedByInterface()
    {
        Assert.True(typeof(IProductCatalogService).IsInterface);
    }
}
