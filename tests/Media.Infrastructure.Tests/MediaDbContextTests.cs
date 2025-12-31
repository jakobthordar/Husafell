using Media.Domain.Assets;
using Media.Infrastructure.Data;
using Media.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Media.Infrastructure.Tests;

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<PostgreSQLTestContainerFixture>
{
}

[Collection("Database")]
public class MediaDbContextTests : IAsyncLifetime
{
    private readonly PostgreSQLTestContainerFixture _fixture;
    private MediaDbContext? _context;
    private readonly ILogger<MediaDbContextTests> _logger;

    public MediaDbContextTests(PostgreSQLTestContainerFixture fixture)
    {
        _fixture = fixture;
        _logger = new ConsoleLogger<MediaDbContextTests>();
    }

    private MediaDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MediaDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .Options;

        return new MediaDbContext(options);
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
    public async Task CanAddAndRetrieveMediaAsset_WithDbContext()
    {
        // Arrange
        var asset = new MediaAsset(
            Guid.NewGuid(),
            "test-image.jpg",
            "image/jpeg");

        // Act
        _context!.MediaAssets.Add(asset);
        await _context.SaveChangesAsync();

        // Assert
        var savedAsset = await _context.MediaAssets
            .FirstOrDefaultAsync(a => a.Id == asset.Id);

        Assert.NotNull(savedAsset);
        Assert.Equal(asset.Id, savedAsset.Id);
        Assert.Equal(asset.FileName, savedAsset.FileName);
        Assert.Equal(asset.ContentType, savedAsset.ContentType);

        _logger.LogInformation("Successfully saved and retrieved media asset with ID: {AssetId}", asset.Id);
    }

    [Fact]
    public async Task CanAddMultipleMediaAssets_WithDbContext()
    {
        // Arrange
        var assets = new[]
        {
            new MediaAsset(Guid.NewGuid(), "image1.jpg", "image/jpeg"),
            new MediaAsset(Guid.NewGuid(), "image2.png", "image/png"),
            new MediaAsset(Guid.NewGuid(), "document.pdf", "application/pdf"),
            new MediaAsset(Guid.NewGuid(), "video.mp4", "video/mp4")
        };

        // Act
        _context!.MediaAssets.AddRange(assets);
        await _context.SaveChangesAsync();

        // Assert
        var savedAssets = await _context.MediaAssets
            .Where(a => assets.Select(asset => asset.Id).Contains(a.Id))
            .ToListAsync();

        Assert.Equal(assets.Length, savedAssets.Count);
        Assert.All(assets, asset => 
            Assert.Contains(savedAssets, saved => saved.Id == asset.Id));

        _logger.LogInformation("Successfully saved {AssetCount} media assets", assets.Length);
    }

    [Fact]
    public async Task CanQueryMediaAssets_ByContentType()
    {
        // Arrange
        var imageAssets = new[]
        {
            new MediaAsset(Guid.NewGuid(), "image1.jpg", "image/jpeg"),
            new MediaAsset(Guid.NewGuid(), "image2.png", "image/png")
        };

        var documentAssets = new[]
        {
            new MediaAsset(Guid.NewGuid(), "document1.pdf", "application/pdf"),
            new MediaAsset(Guid.NewGuid(), "document2.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
        };

        _context!.MediaAssets.AddRange(imageAssets.Concat(documentAssets));
        await _context.SaveChangesAsync();

        // Act
        var allImages = await _context.MediaAssets
            .Where(a => a.ContentType.StartsWith("image/"))
            .ToListAsync();

        var allDocuments = await _context.MediaAssets
            .Where(a => a.ContentType.StartsWith("application/"))
            .ToListAsync();

        // Assert
        Assert.Equal(imageAssets.Length, allImages.Count);
        Assert.Equal(documentAssets.Length, allDocuments.Count);

        Assert.All(imageAssets, asset => 
            Assert.Contains(allImages, img => img.Id == asset.Id));

        Assert.All(documentAssets, asset => 
            Assert.Contains(allDocuments, doc => doc.Id == asset.Id));

        _logger.LogInformation("Successfully queried {ImageCount} images and {DocumentCount} documents", 
            allImages.Count, allDocuments.Count);
    }

    [Fact]
    public async Task CanUpdateMediaAsset_WithDbContext()
    {
        // Arrange
        var asset = new MediaAsset(Guid.NewGuid(), "original-name.jpg", "image/jpeg");
        _context!.MediaAssets.Add(asset);
        await _context.SaveChangesAsync();

        // Act
        var updatedAsset = new MediaAsset(asset.Id, "updated-name.jpg", "image/png");
        _context.MediaAssets.Update(updatedAsset);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedAsset = await _context.MediaAssets
            .FirstOrDefaultAsync(a => a.Id == asset.Id);

        Assert.NotNull(retrievedAsset);
        Assert.Equal("updated-name.jpg", retrievedAsset.FileName);
        Assert.Equal("image/png", retrievedAsset.ContentType);

        _logger.LogInformation("Successfully updated media asset with ID: {AssetId}", asset.Id);
    }

    [Fact]
    public async Task CanDeleteMediaAsset_WithDbContext()
    {
        // Arrange
        var asset = new MediaAsset(Guid.NewGuid(), "to-be-deleted.jpg", "image/jpeg");
        _context!.MediaAssets.Add(asset);
        await _context.SaveChangesAsync();

        // Act
        _context.MediaAssets.Remove(asset);
        await _context.SaveChangesAsync();

        // Assert
        var deletedAsset = await _context.MediaAssets
            .FirstOrDefaultAsync(a => a.Id == asset.Id);

        Assert.Null(deletedAsset);

        _logger.LogInformation("Successfully deleted media asset with ID: {AssetId}", asset.Id);
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