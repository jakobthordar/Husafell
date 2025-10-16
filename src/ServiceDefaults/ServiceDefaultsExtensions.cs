namespace ServiceDefaults;

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class ServiceDefaultsExtensions
{
    public static void AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton(TimeProvider.System);
    }
}
