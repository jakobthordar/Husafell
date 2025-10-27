namespace Catalog.Infrastructure.Tests.Products;

using Catalog.Application.Products;
using Catalog.Domain.Products;
using Microsoft.Extensions.DependencyInjection;

public class ProductCatalogServiceTests : IClassFixture<CatalogDbFixture>
{
    private readonly CatalogDbFixture _fixture;

    public ProductCatalogServiceTests(CatalogDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetProductsAsync_ShouldReturnPersistedProducts()
    {
        await _fixture.ResetDatabaseAsync();

        using var scope = _fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        var product = new Product(Guid.NewGuid(), "Service Product", "Service test product");
        await repository.AddAsync(product);

        using var verificationScope = _fixture.CreateScope();
        var service = verificationScope.ServiceProvider.GetRequiredService<IProductCatalogService>();

        var products = await service.GetProductsAsync();

        Assert.Contains(products, p => p.Id == product.Id);
    }
}
