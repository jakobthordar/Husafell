using Media.Application.Assets;
using Media.Infrastructure.Assets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMediaLibraryService, InMemoryMediaLibraryService>();

var app = builder.Build();

app.MapGet("/library", (IMediaLibraryService service) => Results.Ok(service.GetLibrary()));
app.MapGet("/", () => Results.Ok("Media API"));

app.Run();
