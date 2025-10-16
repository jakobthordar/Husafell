using Catalog.Application.Products;
using Catalog.Infrastructure.Products;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IProductCatalogService, InMemoryProductCatalogService>();

var app = builder.Build();

app.MapGet("/products", (IProductCatalogService service) => Results.Ok(service.GetProducts()));
app.MapGet("/", () => Results.Ok("Catalog API"));

app.Run();
