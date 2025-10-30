using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging;
using Xunit;
using StackExchange.Redis;

namespace Catalog.Infrastructure.Tests.Fixtures;

public class RedisTestContainerFixture : IAsyncLifetime
{
    private readonly TestcontainersContainer _container;
    private readonly ILogger<RedisTestContainerFixture> _logger;
    private IConnectionMultiplexer? _redis;

    public string ConnectionString { get; private set; } = string.Empty;
    public IConnectionMultiplexer Redis => _redis ?? throw new InvalidOperationException("Redis not initialized");

    public RedisTestContainerFixture()
    {
        _logger = new ConsoleLogger<RedisTestContainerFixture>();
        
        _container = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("redis:7-alpine")
            .WithPortBinding(6379, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
            .WithLogger(new TestcontainersLogger(_logger))
            .Build();
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _container.StartAsync();
            
            var port = _container.GetMappedPublicPort(6379);
            ConnectionString = $"localhost:{port}";
            
            var options = ConfigurationOptions.Parse(ConnectionString);
            _redis = await ConnectionMultiplexer.ConnectAsync(options);
            
            _logger.LogInformation("Redis test container started: {ConnectionString}", ConnectionString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Redis test container");
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        try
        {
            _redis?.Dispose();
            await _container.DisposeAsync();
            _logger.LogInformation("Redis test container disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispose Redis test container");
        }
    }
}

[CollectionDefinition("Redis")]
public class RedisCollection : ICollectionFixture<RedisTestContainerFixture>
{
}

[Collection("Redis")]
public class RedisIntegrationTests : IAsyncLifetime
{
    private readonly RedisTestContainerFixture _fixture;
    private readonly ILogger<RedisIntegrationTests> _logger;
    private IDatabase? _database;

    public RedisIntegrationTests(RedisTestContainerFixture fixture)
    {
        _fixture = fixture;
        _logger = new ConsoleLogger<RedisIntegrationTests>();
    }

    public async Task InitializeAsync()
    {
        _database = _fixture.Redis.GetDatabase();
        await _database.KeyFlushAsync(); // Clear any existing data
        _logger.LogInformation("Redis database cleared and ready for testing");
    }

    public Task DisposeAsync()
    {
        // Redis cleanup is handled by the fixture
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CanStoreAndRetrieveString_WithRedis()
    {
        // Arrange
        const string key = "test:key:string";
        const string value = "Hello, Redis!";

        // Act
        await _database!.StringSetAsync(key, value);
        var retrieved = await _database.StringGetAsync(key);

        // Assert
        Assert.Equal(value, retrieved);
        _logger.LogInformation("Successfully stored and retrieved string value");
    }

    [Fact]
    public async Task CanStoreAndRetrieveComplexObject_WithRedis()
    {
        // Arrange
        var key = "test:key:object";
        var testObject = new TestObject
        {
            Id = Guid.NewGuid(),
            Name = "Test Object",
            CreatedAt = DateTime.UtcNow,
            Tags = new[] { "test", "redis", "integration" }
        };

        // Act
        var serialized = JsonSerializer.Serialize(testObject);
        await _database!.StringSetAsync(key, serialized);
        var retrievedJson = await _database.StringGetAsync(key);
        var retrievedObject = JsonSerializer.Deserialize<TestObject>(retrievedJson);

        // Assert
        Assert.NotNull(retrievedObject);
        Assert.Equal(testObject.Id, retrievedObject.Id);
        Assert.Equal(testObject.Name, retrievedObject.Name);
        Assert.Equal(testObject.Tags, retrievedObject.Tags);

        _logger.LogInformation("Successfully stored and retrieved complex object");
    }

    [Fact]
    public async Task CanUseHashOperations_WithRedis()
    {
        // Arrange
        var key = "test:hash:user";
        var userId = "user:123";

        // Act
        await _database!.HashSetAsync(key, new HashEntry[]
        {
            new("id", userId),
            new("name", "John Doe"),
            new("email", "john@example.com"),
            new("last_login", DateTime.UtcNow.ToString("O"))
        });

        var hashEntries = await _database.HashGetAllAsync(key);
        var hash = hashEntries.ToDictionary();

        // Assert
        Assert.Equal(4, hash.Count);
        Assert.Equal(userId, hash["id"]);
        Assert.Equal("John Doe", hash["name"]);
        Assert.Equal("john@example.com", hash["email"]);

        _logger.LogInformation("Successfully stored and retrieved hash data");
    }

    [Fact]
    public async Task CanUseListOperations_WithRedis()
    {
        // Arrange
        var key = "test:list:messages";
        var messages = new[]
        {
            "First message",
            "Second message", 
            "Third message"
        };

        // Act
        foreach (var message in messages)
        {
            await _database!.ListLeftPushAsync(key, message);
        }

        var retrievedMessages = new List<string>();
        var length = await _database.ListLengthAsync(key);
        
        for (int i = 0; i < length; i++)
        {
            var message = await _database.ListGetByIndexAsync(key, i);
            retrievedMessages.Add(message);
        }

        // Assert
        Assert.Equal(messages.Length, retrievedMessages.Count);
        Assert.Equal(messages.Reverse(), retrievedMessages); // LPUSH adds to left, so order is reversed

        _logger.LogInformation("Successfully stored and retrieved list data");
    }

    [Fact]
    public async Task CanUseSetOperations_WithRedis()
    {
        // Arrange
        var key = "test:set:tags";
        var tags = new[] { "csharp", "redis", "testing", "csharp" }; // "csharp" is duplicated

        // Act
        foreach (var tag in tags)
        {
            await _database!.SetAddAsync(key, tag);
        }

        var retrievedTags = await _database.SetMembersAsync(key);

        // Assert
        Assert.Equal(3, retrievedTags.Length); // Duplicates are removed in sets
        Assert.Contains("csharp", retrievedTags);
        Assert.Contains("redis", retrievedTags);
        Assert.Contains("testing", retrievedTags);

        _logger.LogInformation("Successfully stored and retrieved set data");
    }

    [Fact]
    public async Task CanHandleExpiration_WithRedis()
    {
        // Arrange
        var key = "test:expiration";
        const string value = "This will expire";

        // Act
        await _database!.StringSetAsync(key, value, TimeSpan.FromSeconds(2));
        
        // Should exist immediately
        var immediate = await _database.StringGetAsync(key);
        Assert.Equal(value, immediate);

        // Wait for expiration
        await Task.Delay(3000);
        
        var expired = await _database.StringGetAsync(key);

        // Assert
        Assert.True(expired.IsNull);
        _logger.LogInformation("Successfully tested key expiration");
    }
}

public class TestObject
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
}

public static class RedisExtensions
{
    public static Dictionary<string, string> ToDictionary(this HashEntry[] entries)
    {
        return entries.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
    }
}