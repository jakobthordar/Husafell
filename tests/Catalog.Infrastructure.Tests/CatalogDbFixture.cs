namespace Catalog.Infrastructure.Tests;

using Catalog.Infrastructure;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public sealed class CatalogDbFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private ServiceProvider? _serviceProvider;

    public CatalogDbFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("catalog_integration_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithImage("postgres:16-alpine")
            .WithCleanUp(true)
            .Build();
    }

    public IServiceScope CreateScope()
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException("The service provider has not been initialized.");
        }

        return _serviceProvider.CreateScope();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var configuration = BuildConfiguration(_postgresContainer.GetConnectionString());

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddCatalogInfrastructure(configuration);

        _serviceProvider = services.BuildServiceProvider();

        await ApplyMigrationsAsync();
        await ResetDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        if (_serviceProvider is not null)
        {
            await _serviceProvider.DisposeAsync();
        }

        await _postgresContainer.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException("The service provider has not been initialized.");
        }

        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE products RESTART IDENTITY CASCADE;");
    }

    private static IConfiguration BuildConfiguration(string connectionString)
    {
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:CatalogDb"] = connectionString
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    private async Task ApplyMigrationsAsync()
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException("The service provider has not been initialized.");
        }

        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await context.Database.MigrateAsync();
    }
}
