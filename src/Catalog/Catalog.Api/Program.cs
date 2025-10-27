using Catalog.Application.Products;
using Catalog.Infrastructure.Products;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<IProductCatalogService, InMemoryProductCatalogService>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGet("/products", (IProductCatalogService service) => Results.Ok(service.GetProducts()));
app.MapGet("/", () => Results.Ok("Catalog API"));

app.Run();
