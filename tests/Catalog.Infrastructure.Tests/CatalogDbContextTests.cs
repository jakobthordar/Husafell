using Catalog.Domain.Works;
using Catalog.Domain.Works.ValueObjects;
using Catalog.Infrastructure.Data;
using Catalog.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure.Tests;

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<PostgreSQLTestContainerFixture>
{
}

[Collection("Database")]
public class CatalogDbContextTests : IAsyncLifetime
{
    private readonly PostgreSQLTestContainerFixture _fixture;
    private CatalogDbContext? _context;
    private readonly ILogger<CatalogDbContextTests> _logger;

    public CatalogDbContextTests(PostgreSQLTestContainerFixture fixture)
    {
        _fixture = fixture;
        _logger = new ConsoleLogger<CatalogDbContextTests>();
    }

    private CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .Options;

        return new CatalogDbContext(options);
    }

    public async Task InitializeAsync()
    {
        _context = CreateContext();
        await DatabaseMigrationHelper.EnsureDatabaseCreatedAsync(_fixture.ConnectionString);
        _logger.LogInformation("Database created successfully");
    }

    public async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.DisposeAsync();
        }
    }

    [Fact]
    public async Task CanCreateDatabase_WithTestcontainers()
    {
        // Arrange & Act
        var canConnect = await _context!.Database.CanConnectAsync();

        // Assert
        Assert.True(canConnect);
        _logger.LogInformation("Successfully connected to PostgreSQL database via Testcontainers");
    }

    [Fact]
    public async Task CanAddAndRetrieveWork_WithDbContext()
    {
        // Arrange
        var work = Work.Register(
            id: Guid.NewGuid(),
            accessionNumber: AccessionNumber.Create("TEST-001"),
            title: LocalizedText.Create("en", "Test Work"),
            slug: Slug.Create("test-work"),
            description: LocalizedText.Create("en", "A test work for integration testing"),
            dimensions: Dimensions.Create(10.5m, 20.3m, 5.1m, MeasurementUnit.Inches));

        // Act
        _context!.Works.Add(work);
        await _context.SaveChangesAsync();

        // Assert
        var savedWork = await _context.Works
            .Include(w => w.Assets)
            .FirstOrDefaultAsync(w => w.Id == work.Id);

        Assert.NotNull(savedWork);
        Assert.Equal(work.AccessionNumber.Value, savedWork.AccessionNumber.Value);
        Assert.Equal(work.Title.Value, savedWork.Title.Value);
        Assert.Equal(work.Slug.Value, savedWork.Slug.Value);
        
        _logger.LogInformation("Successfully saved and retrieved work with ID: {WorkId}", work.Id);
    }

    [Fact]
    public async Task CanAddWorkWithAssets_WithDbContext()
    {
        // Arrange
        var work = Work.Register(
            id: Guid.NewGuid(),
            accessionNumber: AccessionNumber.Create("TEST-002"),
            title: LocalizedText.Create("en", "Test Work with Assets"),
            slug: Slug.Create("test-work-assets"),
            description: LocalizedText.Create("en", "A test work with assets for integration testing"),
            dimensions: Dimensions.Create(15.0m, 25.0m, 10.0m, MeasurementUnit.Centimetres));

        work.AddAsset(
            assetId: Guid.NewGuid(),
            fileName: "test-image.jpg",
            caption: LocalizedText.Create("en", "Test image caption"),
            isPrimary: true);

        work.AddAsset(
            assetId: Guid.NewGuid(),
            fileName: "test-image-2.jpg",
            caption: LocalizedText.Create("en", "Secondary test image"),
            isPrimary: false);

        // Act
        _context!.Works.Add(work);
        await _context.SaveChangesAsync();

        // Assert
        var savedWork = await _context.Works
            .Include(w => w.Assets)
            .FirstOrDefaultAsync(w => w.Id == work.Id);

        Assert.NotNull(savedWork);
        Assert.Equal(2, savedWork.Assets.Count);
        Assert.True(savedWork.Assets.Any(a => a.IsPrimary));
        Assert.True(savedWork.Assets.Any(a => !a.IsPrimary));

        _logger.LogInformation("Successfully saved work with {AssetCount} assets", savedWork.Assets.Count);
    }

    [Fact]
    public async Task CanQueryWorks_WithFilters()
    {
        // Arrange
        var work1 = Work.Register(
            id: Guid.NewGuid(),
            accessionNumber: AccessionNumber.Create("QUERY-001"),
            title: LocalizedText.Create("en", "Query Test Work 1"),
            slug: Slug.Create("query-test-1"),
            dimensions: Dimensions.Create(10.0m, 20.0m, null, MeasurementUnit.Centimetres));

        var work2 = Work.Register(
            id: Guid.NewGuid(),
            accessionNumber: AccessionNumber.Create("QUERY-002"),
            title: LocalizedText.Create("en", "Query Test Work 2"),
            slug: Slug.Create("query-test-2"),
            dimensions: Dimensions.Create(30.0m, 40.0m, null, MeasurementUnit.Inches));

        _context!.Works.AddRange(work1, work2);
        await _context.SaveChangesAsync();

        // Act
        var allWorks = await _context.Works.ToListAsync();
        var workWithAssets = await _context.Works
            .Include(w => w.Assets)
            .Where(w => w.Assets.Any())
            .ToListAsync();

        // Assert
        Assert.True(allWorks.Count >= 2);
        Assert.Equal(0, workWithAssets.Count); // No assets added to these works

        _logger.LogInformation("Successfully queried {WorkCount} works", allWorks.Count);
    }
}

public class ConsoleLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{logLevel}] [{typeof(T).Name}] {formatter(state, exception)}");
        if (exception != null)
        {
            Console.WriteLine($"Exception: {exception}");
        }
    }
}