using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Testcontainers.PostgreSql;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Catalog.Infrastructure.Data;
using Catalog.Domain.Works;
using Catalog.Domain.Works.ValueObjects;

namespace Catalog.Infrastructure.Tests.Fixtures;

public class ApiIntegrationTestContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private readonly ILogger<ApiIntegrationTestContainerFixture> _logger;
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    public string ConnectionString { get; private set; } = string.Empty;
    public HttpClient Client => _client ?? throw new InvalidOperationException("Client not initialized");

    public ApiIntegrationTestContainerFixture()
    {
        _logger = new ConsoleLogger<ApiIntegrationTestContainerFixture>();
        
        _container = new TestcontainersBuilder<PostgreSqlContainer>()
            .WithDatabase(new PostgreSqlContainerConfiguration
            {
                Database = "catalog_api_test",
                Username = "api_test_user",
                Password = "api_test_password"
            })
            .WithImage("postgres:16-alpine")
            .WithPortBinding(5432, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _container.StartAsync();
            ConnectionString = _container.GetConnectionString();
            
            // Create WebApplicationFactory with test database
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove the default DbContext
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<CatalogDbContext>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        // Add test database
                        services.AddDbContext<CatalogDbContext>(options =>
                            options.UseNpgsql(ConnectionString));

                        // Create the database
                        var sp = services.BuildServiceProvider();
                        using var scope = sp.CreateScope();
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices.GetRequiredService<CatalogDbContext>();
                        
                        db.Database.EnsureCreated();
                        
                        // Seed test data
                        SeedTestData(db);
                    });

                    builder.ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Information);
                    });
                });

            _client = _factory.CreateClient();
            
            _logger.LogInformation("API integration test environment initialized");
            _logger.LogInformation("Database: {ConnectionString}", ConnectionString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize API integration test environment");
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        try
        {
            _client?.Dispose();
            _factory?.Dispose();
            await _container.DisposeAsync();
            _logger.LogInformation("API integration test environment disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispose API integration test environment");
        }
    }

    private void SeedTestData(CatalogDbContext context)
    {
        var testWorks = new[]
        {
            Work.Register(
                id: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                accessionNumber: AccessionNumber.Create("API-TEST-001"),
                title: LocalizedText.Create(("en", "API Test Work 1")),
                slug: Slug.Create("api-test-work-1"),
                description: LocalizedText.Create(("en", "First API test work")),
                dimensions: Dimensions.Create(10.0m, 20.0m, null, MeasurementUnit.Centimetres)),
                
            Work.Register(
                id: Guid.Parse("22222222-2222-2222-2222-222222222222"),
                accessionNumber: AccessionNumber.Create("API-TEST-002"),
                title: LocalizedText.Create(("en", "API Test Work 2")),
                slug: Slug.Create("api-test-work-2"),
                description: LocalizedText.Create(("en", "Second API test work")),
                dimensions: Dimensions.Create(30.0m, 40.0m, null, MeasurementUnit.Inches))
        };

        // Add assets to the first work
        testWorks[0].AddAsset(
            assetId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            fileName: "test-image-1.jpg",
            caption: LocalizedText.Create(("en", "Test image 1")),
            isPrimary: true);

        testWorks[0].AddAsset(
            assetId: Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            fileName: "test-image-2.jpg",
            caption: LocalizedText.Create(("en", "Test image 2")),
            isPrimary: false);

        context.Works.AddRange(testWorks);
        context.SaveChanges();
        
        _logger.LogInformation("Test data seeded successfully");
    }
}

[CollectionDefinition("ApiIntegration")]
public class ApiIntegrationCollection : ICollectionFixture<ApiIntegrationTestContainerFixture>
{
}

[Collection("ApiIntegration")]
public class CatalogApiIntegrationTests : IAsyncLifetime
{
    private readonly ApiIntegrationTestContainerFixture _fixture;
    private readonly ILogger<CatalogApiIntegrationTests> _logger;

    public CatalogApiIntegrationTests(ApiIntegrationTestContainerFixture fixture)
    {
        _fixture = fixture;
        _logger = new ConsoleLogger<CatalogApiIntegrationTests>();
    }

    public Task InitializeAsync()
    {
        _logger.LogInformation("API integration tests initialized");
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _logger.LogInformation("API integration tests completed");
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CanGetAllWorks_ViaApi()
    {
        // Act
        var response = await _fixture.Client.GetAsync("/api/works");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var works = await response.Content.ReadFromJsonAsync<List<WorkDto>>();
        
        Assert.NotNull(works);
        Assert.True(works.Count >= 2); // We seeded 2 works
        
        _logger.LogInformation("Successfully retrieved {WorkCount} works via API", works.Count);
    }

    [Fact]
    public async Task CanGetWorkById_ViaApi()
    {
        // Arrange
        var workId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var response = await _fixture.Client.GetAsync($"/api/works/{workId}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var work = await response.Content.ReadFromJsonAsync<WorkDto>();
        
        Assert.NotNull(work);
        Assert.Equal(workId, work.Id);
        Assert.Equal("API-TEST-001", work.AccessionNumber);
        Assert.Equal("API Test Work 1", work.Title);
        
        _logger.LogInformation("Successfully retrieved work by ID: {WorkId}", workId);
    }

    [Fact]
    public async Task CanCreateWork_ViaApi()
    {
        // Arrange
        var createWorkRequest = new CreateWorkRequest
        {
            AccessionNumber = "API-CREATE-001",
            Title = "API Created Work",
            Slug = "api-created-work",
            Description = "Work created via API test",
            Height = 15.5m,
            Width = 25.5m,
            Unit = "Centimetres"
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync("/api/works", createWorkRequest);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var createdWork = await response.Content.ReadFromJsonAsync<WorkDto>();
        
        Assert.NotNull(createdWork);
        Assert.NotEqual(Guid.Empty, createdWork.Id);
        Assert.Equal(createWorkRequest.AccessionNumber, createdWork.AccessionNumber);
        Assert.Equal(createWorkRequest.Title, createdWork.Title);
        Assert.Equal(createWorkRequest.Slug, createdWork.Slug);
        
        _logger.LogInformation("Successfully created work via API with ID: {WorkId}", createdWork.Id);
    }

    [Fact]
    public async Task CanUpdateWork_ViaApi()
    {
        // Arrange
        var workId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var updateRequest = new UpdateWorkRequest
        {
            Title = "Updated API Test Work 2",
            Description = "Updated description via API test"
        };

        // Act
        var response = await _fixture.Client.PutAsJsonAsync($"/api/works/{workId}", updateRequest);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var updatedWork = await response.Content.ReadFromJsonAsync<WorkDto>();
        
        Assert.NotNull(updatedWork);
        Assert.Equal(workId, updatedWork.Id);
        Assert.Equal(updateRequest.Title, updatedWork.Title);
        Assert.Equal(updateRequest.Description, updatedWork.Description);
        
        _logger.LogInformation("Successfully updated work via API with ID: {WorkId}", workId);
    }

    [Fact]
    public async Task CanDeleteWork_ViaApi()
    {
        // Arrange
        var createRequest = new CreateWorkRequest
        {
            AccessionNumber = "API-DELETE-001",
            Title = "Work to Delete",
            Slug = "work-to-delete"
        };

        var createResponse = await _fixture.Client.PostAsJsonAsync("/api/works", createRequest);
        var createdWork = await createResponse.Content.ReadFromJsonAsync<WorkDto>();
        Assert.NotNull(createdWork);

        // Act
        var deleteResponse = await _fixture.Client.DeleteAsync($"/api/works/{createdWork.Id}");
        
        // Assert
        deleteResponse.EnsureSuccessStatusCode();
        
        // Verify it's deleted
        var getResponse = await _fixture.Client.GetAsync($"/api/works/{createdWork.Id}");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
        
        _logger.LogInformation("Successfully deleted work via API with ID: {WorkId}", createdWork.Id);
    }

    [Fact]
    public async Task CanGetWorkAssets_ViaApi()
    {
        // Arrange
        var workId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var response = await _fixture.Client.GetAsync($"/api/works/{workId}/assets");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var assets = await response.Content.ReadFromJsonAsync<List<AssetDto>>();
        
        Assert.NotNull(assets);
        Assert.Equal(2, assets.Count); // We seeded 2 assets
        Assert.Contains(assets, a => a.IsPrimary);
        Assert.Contains(assets, a => !a.IsPrimary);
        
        _logger.LogInformation("Successfully retrieved {AssetCount} assets for work {WorkId}", assets.Count, workId);
    }
}

// DTOs for API requests/responses
public class WorkDto
{
    public Guid Id { get; set; }
    public string AccessionNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Height { get; set; }
    public decimal? Width { get; set; }
    public decimal? Depth { get; set; }
    public string? Unit { get; set; }
    public List<AssetDto> Assets { get; set; } = new();
}

public class AssetDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public bool IsPrimary { get; set; }
}

public class CreateWorkRequest
{
    public string AccessionNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Height { get; set; }
    public decimal? Width { get; set; }
    public decimal? Depth { get; set; }
    public string? Unit { get; set; }
}

public class UpdateWorkRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? Height { get; set; }
    public decimal? Width { get; set; }
    public decimal? Depth { get; set; }
    public string? Unit { get; set; }
}