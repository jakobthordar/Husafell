# Testcontainers Integration Testing

This project demonstrates comprehensive Testcontainers integration for .NET applications, providing real database and infrastructure testing capabilities.

## 🚀 Quick Start

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or Docker Engine

### Run Integration Tests
```bash
# Run Catalog service integration tests
dotnet test tests/Catalog.Infrastructure.Tests/

# Run Media service integration tests  
dotnet test tests/Media.Infrastructure.Tests/

# Run all tests with detailed output
dotnet test --logger "console;verbosity=detailed"
```

## 📁 Project Structure

```
tests/
├── Catalog.Infrastructure.Tests/
│   ├── Fixtures/
│   │   ├── PostgreSQLTestContainerFixture.cs
│   │   └── RedisTestContainerFixture.cs
│   ├── CatalogDbContextTests.cs
│   └── DatabaseMigrationHelper.cs
├── Media.Infrastructure.Tests/
│   ├── Fixtures/
│   │   └── PostgreSQLTestContainerFixture.cs
│   ├── MediaDbContextTests.cs
│   └── DatabaseMigrationHelper.cs
└── Examples/
    └── TestcontainersIntegrationExamples.md
```

## 🐳 Supported Testcontainers

### PostgreSQL Database
- **Image**: `postgres:16-alpine`
- **Features**: Entity Framework Core integration, full CRUD operations
- **Use Cases**: Domain entity persistence, complex queries, transactions

### Redis Cache
- **Image**: `redis:7-alpine`  
- **Features**: String, Hash, List, Set operations, expiration
- **Use Cases**: Caching, session storage, pub/sub, rate limiting

## 🔧 Key Features

### 1. Automatic Container Management
```csharp
public class PostgreSQLTestContainerFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
    }
    
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
```

### 2. Collection Fixtures for Shared Containers
```csharp
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<PostgreSQLTestContainerFixture>
{
}

[Collection("Database")]
public class MyTests : IAsyncLifetime
{
    public MyTests(PostgreSQLTestContainerFixture fixture) { }
}
```

### 3. Entity Framework Core Integration
```csharp
private CatalogDbContext CreateContext()
{
    var options = new DbContextOptionsBuilder<CatalogDbContext>()
        .UseNpgsql(_fixture.ConnectionString)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .Options;
    return new CatalogDbContext(options);
}
```

### 4. Comprehensive Database Operations
- **Create**: Entity creation with complex value objects
- **Read**: Queries with filtering, includes, projections
- **Update**: Entity modification and tracking
- **Delete**: Cascade operations and soft deletes
- **Relationships**: One-to-many, many-to-many, navigation properties

## 📊 Test Scenarios

### Catalog Service Tests
- ✅ Work entity CRUD operations
- ✅ Asset management and relationships
- ✅ Complex value objects (LocalizedText, Dimensions)
- ✅ Database migrations and schema creation
- ✅ Query operations with LINQ

### Media Service Tests  
- ✅ MediaAsset CRUD operations
- ✅ ContentType-based filtering
- ✅ Batch operations
- ✅ Update and delete scenarios

### Redis Integration Tests
- ✅ String operations (get/set/expire)
- ✅ Complex object serialization
- ✅ Hash operations (field/value pairs)
- ✅ List operations (queues/stacks)
- ✅ Set operations (unique collections)
- ✅ Key expiration and TTL

## 🎯 Benefits

### 1. Real Environment Testing
- Test against actual PostgreSQL and Redis instances
- No more in-memory database limitations
- Real SQL execution and query optimization
- Database-specific data types and constraints

### 2. Test Isolation
- Each test suite gets its own container
- Automatic cleanup prevents test interference
- Parallel test execution support
- Consistent test environments

### 3. Developer Experience
- Fast container startup with Alpine images
- Comprehensive logging for debugging
- Automatic port management
- IDE integration with test runners

### 4. CI/CD Ready
- Works in Docker-based pipelines
- No external dependencies
- Consistent environments across machines
- Easy integration with GitHub Actions, Azure DevOps, etc.

## 🔍 Configuration

### PostgreSQL Container Configuration
```csharp
new TestcontainersBuilder<PostgreSqlTestcontainer>()
    .WithDatabase(new PostgreSqlTestcontainerConfiguration
    {
        Database = "test_catalog",
        Username = "test_user", 
        Password = "test_password"
    })
    .WithImage("postgres:16-alpine")
    .WithPortBinding(5432, true)
    .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
```

### Redis Container Configuration
```csharp
new TestcontainersBuilder<TestcontainersContainer>()
    .WithImage("redis:7-alpine")
    .WithPortBinding(6379, true)
    .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
```

## 📝 Writing New Tests

### 1. Create Test Class
```csharp
[Collection("Database")]
public class MyFeatureTests : IAsyncLifetime
{
    private readonly PostgreSQLTestContainerFixture _fixture;
    private MyDbContext? _context;

    public MyFeatureTests(PostgreSQLTestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _context = CreateContext();
        await DatabaseMigrationHelper.EnsureDatabaseCreatedAsync(_fixture.ConnectionString);
    }
}
```

### 2. Write Test Methods
```csharp
[Fact]
public async Task MyFeature_ShouldWork_WithRealDatabase()
{
    // Arrange
    var entity = new MyEntity("test data");
    
    // Act
    _context!.MyEntities.Add(entity);
    await _context.SaveChangesAsync();
    
    // Assert
    var saved = await _context.MyEntities.FindAsync(entity.Id);
    Assert.NotNull(saved);
    Assert.Equal("test data", saved.Name);
}
```

### 3. Add New Testcontainers
1. Create fixture class implementing `IAsyncLifetime`
2. Configure container with appropriate image and settings
3. Add collection definition for sharing
4. Update test project with required NuGet packages

## 🐛 Troubleshooting

### Common Issues

#### Docker Not Running
```
Error: Docker container failed to start
```
**Solution**: Ensure Docker Desktop is running and accessible

#### Port Conflicts
Testcontainers automatically handles port conflicts by binding to random available ports.

#### Connection Timeouts
```
Error: Connection timeout to database
```
**Solution**: Increase wait strategy timeout or check container logs

#### Migration Issues
```
Error: Database creation failed
```
**Solution**: Check entity configurations and ensure proper EF Core setup

### Debugging Tips

#### Enable Verbose Logging
```bash
dotnet test --logger "console;verbosity=detailed"
```

#### Check Container Logs
```bash
docker ps  # Find container ID
docker logs <container_id>
```

#### Connect Directly to Database
Use the connection string from test logs to connect with pgAdmin or other tools.

## 🚀 Advanced Usage

### Custom Container Images
```csharp
.WithImage("mcr.microsoft.com/mssql/server:2022-latest")
.WithEnvironment("ACCEPT_EULA", "Y")
.WithEnvironment("SA_PASSWORD", "Your_password123")
```

### Multiple Containers
```csharp
[CollectionDefinition("FullStack")]
public class FullStackCollection : ICollectionFixture<FullStackFixture>
{
}

public class FullStackFixture : IAsyncLifetime
{
    private readonly PostgreSqlTestcontainer _postgres;
    private readonly TestcontainersContainer _redis;
    
    // Initialize both containers...
}
```

### Volume Mounting
```csharp
.WithBindMount("/path/to/host/data", "/var/lib/postgresql/data")
```

### Network Configuration
```csharp
.WithNetwork("test-network")
.WithNetworkAliases("postgres", "redis")
```

## 📚 Additional Resources

- [Testcontainers .NET Documentation](https://dotnet.testcontainers.org/)
- [Entity Framework Core Testing](https://docs.microsoft.com/en-us/ef/core/testing/)
- [Docker Documentation](https://docs.docker.com/)
- [xUnit Documentation](https://xunit.net/docs/)

## 🤝 Contributing

When adding new Testcontainers:

1. Follow existing naming conventions
2. Add comprehensive logging
3. Include cleanup in disposal
4. Add appropriate documentation
5. Update this README with new examples

## 📄 License

This project follows the same license as the main repository.