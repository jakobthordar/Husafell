using Catalog.Application.Works;
using Catalog.Infrastructure.Works;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<IWorkCatalogService, InMemoryWorkCatalogService>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGet("/works", (IWorkCatalogService service) => Results.Ok(service.GetWorks()));
app.MapGet("/", () => Results.Ok("Catalog API"));

app.Run();
