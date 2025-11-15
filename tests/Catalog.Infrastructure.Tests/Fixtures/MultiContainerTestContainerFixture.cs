using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Microsoft.Extensions.Logging;
using Xunit;
using StackExchange.Redis;
using System.Text.Json;
using Catalog.Infrastructure.Data;
using Catalog.Domain.Works;
using Catalog.Domain.Works.ValueObjects;

namespace Catalog.Infrastructure.Tests.Fixtures;

public class MultiContainerTestContainerFixture : IAsyncLifetime
{
    private readonly INetwork _network;
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RedisContainer _redisContainer;
    private readonly ILogger<MultiContainerTestContainerFixture> _logger;
    private IConnectionMultiplexer? _redis;

    public string PostgreSQLConnectionString { get; private set; } = string.Empty;
    public IConnectionMultiplexer Redis => _redis ?? throw new InvalidOperationException("Redis not initialized");

    public MultiContainerTestContainerFixture()
    {
        _logger = new ConsoleLogger<MultiContainerTestContainerFixture>();
        
        // Create a shared network for container communication
        _network = new TestcontainersNetworkBuilder()
            .WithName("test-network")
            .Build();

        // PostgreSQL container
        _postgresContainer = new TestcontainersBuilder<PostgreSqlContainer>()
            .WithNetwork(_network)
            .WithNetworkAliases("catalog-db")
            .WithDatabase(new PostgreSqlContainerConfiguration
            {
                Database = "catalog_integration",
                Username = "integration_user",
                Password = "integration_password"
            })
            .WithImage("postgres:16-alpine")
            .WithPortBinding(5432, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();

        // Redis container
        _redisContainer = new TestcontainersBuilder<RedisContainer>()
            .WithNetwork(_network)
            .WithNetworkAliases("catalog-cache")
            .WithImage("redis:7-alpine")
            .WithPortBinding(6379, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
            .Build();
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Start network first
            await _network.CreateAsync();
            _logger.LogInformation("Test network created successfully");

            // Start containers
            await _postgresContainer.StartAsync();
            await _redisContainer.StartAsync();

            PostgreSQLConnectionString = _postgresContainer.GetConnectionString();
            
            var redisPort = _redisContainer.GetMappedPublicPort(6379);
            var redisConnectionString = $"localhost:{redisPort}";
            var options = ConfigurationOptions.Parse(redisConnectionString);
            _redis = await ConnectionMultiplexer.ConnectAsync(options);

            _logger.LogInformation("Multi-container setup completed:");
            _logger.LogInformation("PostgreSQL: {ConnectionString}", PostgreSQLConnectionString);
            _logger.LogInformation("Redis: {ConnectionString}", redisConnectionString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize multi-container setup");
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        try
        {
            _redis?.Dispose();
            await _postgresContainer.DisposeAsync();
            await _redisContainer.DisposeAsync();
            await _network.DeleteAsync();
            _logger.LogInformation("Multi-container setup disposed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispose multi-container setup");
        }
    }
}

[CollectionDefinition("MultiContainer")]
public class MultiContainerCollection : ICollectionFixture<MultiContainerTestContainerFixture>
{
}

[Collection("MultiContainer")]
public class MultiContainerIntegrationTests : IAsyncLifetime
{
    private readonly MultiContainerTestContainerFixture _fixture;
    private readonly ILogger<MultiContainerIntegrationTests> _logger;
    private CatalogDbContext? _catalogContext;
    private IDatabase? _redisDatabase;

    public MultiContainerIntegrationTests(MultiContainerTestContainerFixture fixture)
    {
        _fixture = fixture;
        _logger = new ConsoleLogger<MultiContainerIntegrationTests>();
    }

    public async Task InitializeAsync()
    {
        // Initialize database context
        var dbOptions = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(_fixture.PostgreSQLConnectionString)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .Options;
        _catalogContext = new CatalogDbContext(dbOptions);
        await DatabaseMigrationHelper.EnsureDatabaseCreatedAsync(_fixture.PostgreSQLConnectionString);

        // Initialize Redis
        _redisDatabase = _fixture.Redis.GetDatabase();
        await _redisDatabase.KeyFlushAsync();

        _logger.LogInformation("Multi-container tests initialized successfully");
    }

    public async Task DisposeAsync()
    {
        if (_catalogContext != null)
        {
            await _catalogContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task CanUsePostgresAndRedisTogether_ForCachingScenario()
    {
        // Arrange
        var work = Work.Register(
            id: Guid.NewGuid(),
            accessionNumber: AccessionNumber.Create("CACHE-001"),
            title: LocalizedText.Create(("en", "Cached Work")),
            slug: Slug.Create("cached-work"),
            description: LocalizedText.Create(("en", "A work that will be cached")),
            dimensions: Dimensions.Create(20.0m, 30.0m, null, MeasurementUnit.Centimetres));

        // Act - Save to database
        _catalogContext!.Works.Add(work);
        await _catalogContext.SaveChangesAsync();

        // Cache the work in Redis
        var cacheKey = $"work:{work.Id}";
        var cachedWork = new CachedWork
        {
            Id = work.Id,
            Title = work.Title.Value,
            AccessionNumber = work.AccessionNumber.Value,
            Slug = work.Slug.Value,
            CachedAt = DateTime.UtcNow
        };

        await _redisDatabase!.StringSetAsync(cacheKey, JsonSerializer.Serialize(cachedWork), TimeSpan.FromMinutes(5));

        // Retrieve from cache
        var cachedJson = await _redisDatabase.StringGetAsync(cacheKey);
        var retrievedFromCache = JsonSerializer.Deserialize<CachedWork>(cachedJson);

        // Retrieve from database for comparison
        var fromDatabase = await _catalogContext.Works.FindAsync(work.Id);

        // Assert
        Assert.NotNull(retrievedFromCache);
        Assert.NotNull(fromDatabase);
        Assert.Equal(work.Id, retrievedFromCache.Id);
        Assert.Equal(work.Title.Value, retrievedFromCache.Title);
        Assert.Equal(work.AccessionNumber.Value, retrievedFromCache.AccessionNumber);

        _logger.LogInformation("Successfully demonstrated PostgreSQL + Redis caching scenario");
    }

    [Fact]
    public async Task CanImplementCacheAsidePattern_WithDatabaseAndRedis()
    {
        // Arrange
        var workId = Guid.NewGuid();
        var cacheKey = $"work:{workId}";

        // Act & Assert - Try to get from cache first (should be empty)
        var cachedResult = await _redisDatabase!.StringGetAsync(cacheKey);
        Assert.True(cachedResult.IsNull, "Cache should be empty initially");

        // Get from database
        var work = Work.Register(
            id: workId,
            accessionNumber: AccessionNumber.Create("ASIDE-001"),
            title: LocalizedText.Create(("en", "Cache-Aside Work")),
            slug: Slug.Create("cache-aside-work"),
            dimensions: Dimensions.Create(15.0m, 25.0m, null, MeasurementUnit.Inches));

        _catalogContext!.Works.Add(work);
        await _catalogContext.SaveChangesAsync();

        // Cache the result
        var fromDb = await _catalogContext.Works.FindAsync(workId);
        var cacheData = new CachedWork
        {
            Id = fromDb.Id,
            Title = fromDb.Title.Value,
            AccessionNumber = fromDb.AccessionNumber.Value,
            Slug = fromDb.Slug.Value,
            CachedAt = DateTime.UtcNow
        };

        await _redisDatabase.StringSetAsync(cacheKey, JsonSerializer.Serialize(cacheData));

        // Now try cache again (should have data)
        cachedResult = await _redisDatabase.StringGetAsync(cacheKey);
        Assert.False(cachedResult.IsNull, "Cache should contain data");

        var cachedWork = JsonSerializer.Deserialize<CachedWork>(cachedResult);
        Assert.Equal(work.Id, cachedWork.Id);
        Assert.Equal(work.Title.Value, cachedWork.Title);

        _logger.LogInformation("Successfully implemented cache-aside pattern");
    }

    [Fact]
    public async Task CanDemonstrateWriteThroughCaching_WithBothContainers()
    {
        // Arrange
        var work = Work.Register(
            id: Guid.NewGuid(),
            accessionNumber: AccessionNumber.Create("WRITETHRU-001"),
            title: LocalizedText.Create(("en", "Write-Through Work")),
            slug: Slug.Create("write-through-work"),
            dimensions: Dimensions.Create(10.0m, 20.0m, null, MeasurementUnit.Centimetres));

        // Act - Write to both database and cache simultaneously
        _catalogContext!.Works.Add(work);
        await _catalogContext.SaveChangesAsync();

        var cacheKey = $"work:{work.Id}";
        var cacheData = new CachedWork
        {
            Id = work.Id,
            Title = work.Title.Value,
            AccessionNumber = work.AccessionNumber.Value,
            Slug = work.Slug.Value,
            CachedAt = DateTime.UtcNow
        };

        await _redisDatabase!.StringSetAsync(cacheKey, JsonSerializer.Serialize(cacheData));

        // Verify both contain the data
        var fromDb = await _catalogContext.Works.FindAsync(work.Id);
        var fromCache = JsonSerializer.Deserialize<CachedWork>(await _redisDatabase.StringGetAsync(cacheKey));

        // Assert
        Assert.NotNull(fromDb);
        Assert.NotNull(fromCache);
        Assert.Equal(fromDb.Id, fromCache.Id);
        Assert.Equal(fromDb.Title.Value, fromCache.Title);
        Assert.Equal(fromDb.AccessionNumber.Value, fromCache.AccessionNumber);

        _logger.LogInformation("Successfully demonstrated write-through caching");
    }

    [Fact]
    public async Task CanHandleCacheInvalidation_WhenDatabaseChanges()
    {
        // Arrange
        var work = Work.Register(
            id: Guid.NewGuid(),
            accessionNumber: AccessionNumber.Create("INVALIDATE-001"),
            title: LocalizedText.Create(("en", "Original Title")),
            slug: Slug.Create("original-title"),
            dimensions: Dimensions.Create(25.0m, 35.0m, null, MeasurementUnit.Centimetres));

        _catalogContext!.Works.Add(work);
        await _catalogContext.SaveChangesAsync();

        var cacheKey = $"work:{work.Id}";
        var originalCache = new CachedWork
        {
            Id = work.Id,
            Title = work.Title.Value,
            AccessionNumber = work.AccessionNumber.Value,
            Slug = work.Slug.Value,
            CachedAt = DateTime.UtcNow
        };

        await _redisDatabase!.StringSetAsync(cacheKey, JsonSerializer.Serialize(originalCache));

        // Verify cache has original data
        var cachedBefore = JsonSerializer.Deserialize<CachedWork>(await _redisDatabase.StringGetAsync(cacheKey));
        Assert.Equal("Original Title", cachedBefore.Title);

        // Act - Update the database and invalidate cache
        work.UpdateTitle(LocalizedText.Create(("en", "Updated Title")));
        await _catalogContext.SaveChangesAsync();

        // Invalidate cache
        await _redisDatabase.KeyDeleteAsync(cacheKey);

        // Verify cache is empty
        var cachedAfterInvalidation = await _redisDatabase.StringGetAsync(cacheKey);
        Assert.True(cachedAfterInvalidation.IsNull, "Cache should be empty after invalidation");

        // Verify database has updated data
        var fromDb = await _catalogContext.Works.FindAsync(work.Id);
        Assert.Equal("Updated Title", fromDb.Title.Value);

        _logger.LogInformation("Successfully demonstrated cache invalidation");
    }
}

public class CachedWork
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AccessionNumber { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime CachedAt { get; set; }
}