using Media.Application.Assets;
using Media.Infrastructure.Assets;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddSingleton<IMediaLibraryService, InMemoryMediaLibraryService>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGet("/library", (IMediaLibraryService service) => Results.Ok(service.GetLibrary()));
app.MapGet("/", () => Results.Ok("Media API"));

app.Run();
