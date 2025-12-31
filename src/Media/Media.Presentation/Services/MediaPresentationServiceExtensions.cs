namespace Media.Presentation.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Extension methods for configuring Media presentation services.
/// </summary>
public static class MediaPresentationServiceExtensions
{
    /// <summary>
    /// Adds Media presentation services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediaPresentation(this IServiceCollection services)
    {
        services.AddHttpClient<MediaApiService>(client =>
        {
            client.BaseAddress = new Uri("http://media-api");
        });
        return services;
    }
}
