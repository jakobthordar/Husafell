namespace Catalog.Infrastructure;

using Catalog.Application.Products;
using Catalog.Infrastructure.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CatalogDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'CatalogDb' was not found.");
        }

        services.AddDbContext<CatalogDbContext>(options =>
        {
            options.UseNpgsql(connectionString, builder => builder.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName));
            options.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductCatalogService, ProductCatalogService>();

        return services;
    }
}
