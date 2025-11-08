# Testcontainers Integration Examples

This directory contains examples of how to use Testcontainers for integration testing with PostgreSQL databases in .NET applications.

## Overview

The examples demonstrate:

1. **PostgreSQL Testcontainers**: Using real PostgreSQL databases in tests
2. **Entity Framework Core Integration**: Testing EF Core DbContext with actual databases
3. **Collection Fixtures**: Shared database containers across multiple test classes
4. **Database Migration**: Ensuring database schema is created for tests
5. **Clean Architecture**: Integration tests following Clean Architecture principles

## Examples

### 1. Catalog Service Integration Tests

- **Location**: `../Catalog.Infrastructure.Tests/CatalogDbContextTests.cs`
- **Database**: PostgreSQL
- **Features**:
  - Works and Assets entity testing
  - Complex value objects (LocalizedText, Dimensions, etc.)
  - Entity relationships and navigation properties
  - CRUD operations with real database

### 2. Media Service Integration Tests

- **Location**: `../Media.Infrastructure.Tests/MediaDbContextTests.cs`
- **Database**: PostgreSQL
- **Features**:
  - MediaAsset entity testing
  - Query operations with filtering
  - Update and delete operations
  - ContentType-based queries

### 3. Testcontainer Fixtures

- **Location**: `../Catalog.Infrastructure.Tests/Fixtures/PostgreSQLTestContainerFixture.cs`
- **Location**: `../Media.Infrastructure.Tests/Fixtures/PostgreSQLTestContainerFixture.cs`
- **Features**:
  - Automatic container lifecycle management
  - Connection string generation
  - Logging and error handling
  - Port binding and wait strategies

## Key Benefits

### 1. Real Database Testing
- Tests run against actual PostgreSQL instances
- No more in-memory database limitations
- Real SQL execution and query optimization
- Database-specific data types and constraints

### 2. Isolation and Reliability
- Each test suite gets its own database container
- Containers are automatically cleaned up after tests
- No test interference or state leakage
- Parallel test execution support

### 3. Development Experience
- Fast container startup with PostgreSQL Alpine images
- Comprehensive logging for debugging
- Connection string management
- Automatic port binding

### 4. CI/CD Ready
- Works in Docker-based CI/CD pipelines
- No external database dependencies
- Consistent test environments across machines
- Easy integration with test runners

## Usage Patterns

### Basic Test Structure

```csharp
[Collection("Database")]
public class MyIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSQLTestContainerFixture _fixture;
    private MyDbContext? _context;

    public MyIntegrationTests(PostgreSQLTestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _context = CreateContext();
        await DatabaseMigrationHelper.EnsureDatabaseCreatedAsync(_fixture.ConnectionString);
    }

    [Fact]
    public async Task MyTest()
    {
        // Arrange
        var entity = new MyEntity("test");
        
        // Act
        _context.MyEntities.Add(entity);
        await _context.SaveChangesAsync();
        
        // Assert
        var saved = await _context.MyEntities.FindAsync(entity.Id);
        Assert.NotNull(saved);
    }
}
```

### Collection Fixture Setup

```csharp
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<PostgreSQLTestContainerFixture>
{
}
```

## Running the Tests

### Prerequisites
- Docker Desktop or Docker Engine running
- .NET 9.0 SDK

### Run Tests
```bash
dotnet test tests/Catalog.Infrastructure.Tests/
dotnet test tests/Media.Infrastructure.Tests/
```

### Run with Detailed Output
```bash
dotnet test tests/Catalog.Infrastructure.Tests/ --logger "console;verbosity=detailed"
```

## Configuration

### Container Configuration
The PostgreSQL containers are configured with:
- Database: `test_catalog` or `test_media`
- Username: `test_user`
- Password: `test_password`
- Image: `postgres:16-alpine`
- Auto port binding

### Connection String
Connection strings are automatically generated:
```
Host=localhost;Port={random_port};Database=test_catalog;Username=test_user;Password=test_password
```

## Best Practices

### 1. Test Data Management
- Use `IAsyncLifetime` for setup/teardown
- Create fresh data for each test when possible
- Use transactions for test isolation when needed

### 2. Performance
- Use collection fixtures to share containers
- Minimize container startup/teardown overhead
- Consider parallel test execution

### 3. Debugging
- Enable detailed logging in testcontainers
- Use database tools to inspect test data
- Check container logs for SQL issues

### 4. CI/CD Integration
- Ensure Docker is available in CI environment
- Use appropriate timeout values for container startup
- Handle container cleanup properly

## Extending the Examples

### Adding New Testcontainers
1. Create new fixture classes for other databases (MySQL, SQL Server, etc.)
2. Update test project references
3. Create corresponding DbContext classes
4. Add migration helpers

### Adding More Test Scenarios
1. Test complex queries and joins
2. Test database transactions
3. Test concurrent operations
4. Test database constraints and validations

## Troubleshooting

### Common Issues
1. **Docker not running**: Ensure Docker Desktop is running
2. **Port conflicts**: Testcontainers handles this automatically
3. **Connection timeouts**: Increase wait strategy timeout
4. **Migration issues**: Check entity configurations

### Debugging Tips
- Enable verbose logging in Testcontainers
- Check container logs with `docker logs <container_id>`
- Use database tools to connect directly to test containers
- Verify entity configurations match database schema