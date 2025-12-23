using Microsoft.Extensions.DependencyInjection;

namespace Shared.Web.Services;

public static class SharedWebServiceExtensions
{
    public static IServiceCollection AddSharedWebServices(this IServiceCollection services)
    {
        services.AddScoped<NotificationService>();
        
        return services;
    }
}
