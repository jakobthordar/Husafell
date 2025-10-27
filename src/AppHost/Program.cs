using Aspire.Hosting;
using Aspire.Hosting.Azure;
using Aspire.Hosting.PostgreSQL;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var environmentName = builder.Environment.EnvironmentName ?? Environments.Development;
const string storageConnectionString = "UseDevelopmentStorage=true";
const string catalogDbConnectionString = "Host=catalog-db;Port=5432;Username=catalog;Password=catalog_password;Database=catalog";
const string mediaDbConnectionString = "Host=media-db;Port=5432;Username=media;Password=media_password;Database=media";

var catalogDbServer = builder.AddPostgres("catalog-db")
    .WithDataVolume()
    .WithEnvironment("POSTGRES_USER", "catalog")
    .WithEnvironment("POSTGRES_PASSWORD", "catalog_password")
    .WithEnvironment("POSTGRES_DB", "catalog");

var catalogDatabase = catalogDbServer.AddDatabase("catalog");

var mediaDbServer = builder.AddPostgres("media-db")
    .WithDataVolume()
    .WithEnvironment("POSTGRES_USER", "media")
    .WithEnvironment("POSTGRES_PASSWORD", "media_password")
    .WithEnvironment("POSTGRES_DB", "media");

var mediaDatabase = mediaDbServer.AddDatabase("media");

var storage = builder.AddAzureStorage("storage");
var blobStorage = storage.AddBlobs("media-assets");

var catalogApi = builder.AddProject<Projects.Catalog_Api>("catalog-api")
    .WithReference(catalogDatabase)
    .WithReference(blobStorage)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
    .WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
    .WithEnvironment("ConnectionStrings__Catalog", catalogDbConnectionString)
    .WithEnvironment("ConnectionStrings__Storage", storageConnectionString)
    .WithEnvironment("Storage__ConnectionString", storageConnectionString)
    .WithEnvironment("ServiceDiscovery__Services__CatalogApi", "http://catalog-api")
    .WithEnvironment("ServiceDiscovery__Services__MediaApi", "http://media-api");

var mediaApi = builder.AddProject<Projects.Media_Api>("media-api")
    .WithReference(mediaDatabase)
    .WithReference(blobStorage)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
    .WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
    .WithEnvironment("ConnectionStrings__Media", mediaDbConnectionString)
    .WithEnvironment("ConnectionStrings__Storage", storageConnectionString)
    .WithEnvironment("Storage__ConnectionString", storageConnectionString)
    .WithEnvironment("ServiceDiscovery__Services__CatalogApi", "http://catalog-api")
    .WithEnvironment("ServiceDiscovery__Services__MediaApi", "http://media-api");

builder.Build().Run();
