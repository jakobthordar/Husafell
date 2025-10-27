namespace Catalog.Infrastructure.Tests.Products;

using Catalog.Application.Products;
using Catalog.Domain.Products;
using Microsoft.Extensions.DependencyInjection;

public class ProductRepositoryTests : IClassFixture<CatalogDbFixture>
{
    private readonly CatalogDbFixture _fixture;

    public ProductRepositoryTests(CatalogDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_ShouldPersistProduct()
    {
        await _fixture.ResetDatabaseAsync();

        using var scope = _fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        var product = new Product(Guid.NewGuid(), "Integration Test Product", "An integration test product.");

        await repository.AddAsync(product);

        using var verificationScope = _fixture.CreateScope();
        var persisted = await verificationScope.ServiceProvider
            .GetRequiredService<IProductRepository>()
            .GetByIdAsync(product.Id);

        Assert.NotNull(persisted);
        Assert.Equal(product.Name, persisted!.Name);
        Assert.Equal(product.Description, persisted.Description);
    }

    [Fact]
    public async Task ListAsync_ShouldReturnPersistedProducts()
    {
        await _fixture.ResetDatabaseAsync();

        using var scope = _fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        var product = new Product(Guid.NewGuid(), "Another Product", "Another integration product.");

        await repository.AddAsync(product);

        var products = await repository.ListAsync();

        Assert.Contains(products, p => p.Id == product.Id);
    }
}
