var builder = DistributedApplication.CreateBuilder(args);

var environmentName = builder.Environment.EnvironmentName;

var catalogDbUser = builder.AddParameter("catalog-db-user", "catalog", publishValueAsDefault: true, secret: false);
var catalogDbPassword = builder.AddParameter(
    "catalog-db-password",
    new GenerateParameterDefault
    {
        MinLength = 16,
        Lower = true,
        Upper = true,
        Numeric = true,
        Special = true,
        MinLower = 1,
        MinUpper = 1,
        MinNumeric = 1,
        MinSpecial = 1,
    },
    secret: true,
    persist: true);

var mediaDbUser = builder.AddParameter("media-db-user", "media", publishValueAsDefault: true, secret: false);
var mediaDbPassword = builder.AddParameter(
    "media-db-password",
    new GenerateParameterDefault
    {
        MinLength = 16,
        Lower = true,
        Upper = true,
        Numeric = true,
        Special = true,
        MinLower = 1,
        MinUpper = 1,
        MinNumeric = 1,
        MinSpecial = 1,
    },
    secret: true,
    persist: true);

var catalogDbServer = builder
    .AddPostgres("catalog-db", catalogDbUser, catalogDbPassword)
    .WithDataVolume();

var catalogDatabase = catalogDbServer
    .AddDatabase("catalog");

var mediaDbServer = builder
    .AddPostgres("media-db", mediaDbUser, mediaDbPassword)
    .WithDataVolume();

var mediaDatabase = mediaDbServer.AddDatabase("media");

var catalogApi = builder
    .AddProject<Projects.Catalog_Api>("catalog-api")
    .WithReference(catalogDatabase)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
    .WithEnvironment("DOTNET_ENVIRONMENT", environmentName);

var mediaApi = builder
    .AddProject<Projects.Media_Api>("media-api")
    .WithReference(mediaDatabase)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
    .WithEnvironment("DOTNET_ENVIRONMENT", environmentName);

catalogApi.WithReference(mediaApi);
mediaApi.WithReference(catalogApi);

builder.Build().Run();