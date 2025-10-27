namespace ServiceDefaults;

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

public static class ServiceDefaultsExtensions
{
    private const string SharedConfigurationFile = "appsettings.Shared.json";
    private const string EnvironmentConfigurationFormat = "appsettings.{0}.json";
    private const string ServiceDiscoverySection = "ServiceDiscovery:Services";

    public static void AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Configuration.AddJsonFile(SharedConfigurationFile, optional: true, reloadOnChange: true);

        if (!string.IsNullOrWhiteSpace(builder.Environment.EnvironmentName))
        {
            builder.Configuration.AddJsonFile(
                string.Format(EnvironmentConfigurationFormat, builder.Environment.EnvironmentName),
                optional: true,
                reloadOnChange: true);
        }

        builder.Services.AddSingleton(TimeProvider.System);

        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());

        builder.Services.AddHttpClient();

        builder.Services.Configure<ServiceDiscoveryOptions>(builder.Configuration.GetSection(ServiceDiscoverySection));
        builder.Services.AddSingleton<IConfigureOptions<HttpClientFactoryOptions>, ServiceDiscoveryHttpClientFactoryOptionsSetup>();
    }

    public static void MapDefaultEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapHealthChecks("/health");
    }

    private sealed class ServiceDiscoveryHttpClientFactoryOptionsSetup : IConfigureNamedOptions<HttpClientFactoryOptions>
    {
        private readonly IOptionsMonitor<ServiceDiscoveryOptions> _options;

        public ServiceDiscoveryHttpClientFactoryOptionsSetup(IOptionsMonitor<ServiceDiscoveryOptions> options)
        {
            _options = options;
        }

        public void Configure(HttpClientFactoryOptions options)
            => Configure(Options.DefaultName, options);

        public void Configure(string? name, HttpClientFactoryOptions options)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            var serviceUri = _options.CurrentValue.GetServiceUri(name);
            if (serviceUri is null)
            {
                return;
            }

            options.HttpClientActions.Add(client => client.BaseAddress = serviceUri);
        }
    }

    private sealed class ServiceDiscoveryOptions
    {
        public Dictionary<string, string> Services { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        public Uri? GetServiceUri(string name)
        {
            if (TryResolve(name, out var resolved))
            {
                return resolved;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var sanitized = RemoveNonAlphaNumeric(name);
            if (!string.Equals(sanitized, name, StringComparison.OrdinalIgnoreCase) && TryResolve(sanitized, out resolved))
            {
                return resolved;
            }

            var pascal = ToPascalCase(name);
            return !string.Equals(pascal, name, StringComparison.OrdinalIgnoreCase) && TryResolve(pascal, out resolved)
                ? resolved
                : null;
        }

        private bool TryResolve(string? key, out Uri? uri)
        {
            if (!string.IsNullOrWhiteSpace(key)
                && Services.TryGetValue(key, out var value)
                && Uri.TryCreate(value, UriKind.Absolute, out var parsed))
            {
                uri = parsed;
                return true;
            }

            uri = null;
            return false;
        }

        private static string RemoveNonAlphaNumeric(string value)
        {
            var builder = new StringBuilder(value.Length);

            foreach (var character in value)
            {
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(character);
                }
            }

            return builder.ToString();
        }

        private static string ToPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var builder = new StringBuilder(value.Length);
            var shouldUppercase = true;

            foreach (var character in value)
            {
                if (character is '-' or '_' or '.')
                {
                    shouldUppercase = true;
                    continue;
                }

                builder.Append(shouldUppercase ? char.ToUpperInvariant(character) : character);
                shouldUppercase = false;
            }

            return builder.ToString();
        }
    }
}
