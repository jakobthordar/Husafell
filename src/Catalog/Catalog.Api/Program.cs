using Catalog.Application.Products;
using Catalog.Infrastructure;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddCatalogInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGet("/products", async (IProductCatalogService service, CancellationToken cancellationToken) =>
{
    var products = await service.GetProductsAsync(cancellationToken);
    return Results.Ok(products);
});
app.MapGet("/", () => Results.Ok("Catalog API"));

app.Run();
