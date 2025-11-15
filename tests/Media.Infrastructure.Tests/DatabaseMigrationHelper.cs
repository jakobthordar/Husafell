using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Media.Infrastructure.Data;

namespace Media.Infrastructure.Tests;

public class DatabaseMigrationHelper
{
    public static async Task EnsureDatabaseCreatedAsync(string connectionString)
    {
        var services = new ServiceCollection();
        
        services.AddLogging(builder => builder.AddConsole());
        
        services.AddDbContext<MediaDbContext>(options =>
            options.UseNpgsql(connectionString));

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
        
        await context.Database.EnsureCreatedAsync();
    }
}