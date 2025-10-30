using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure.Tests;

public class DatabaseMigrationHelper
{
    public static async Task EnsureDatabaseCreatedAsync(string connectionString)
    {
        var services = new ServiceCollection();
        
        services.AddLogging(builder => builder.AddConsole());
        
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connectionString));

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        
        await context.Database.EnsureCreatedAsync();
    }
}