using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaExampleProject.Features.Settings;

public static class SettingsFeature
{
    public static IServiceCollection AddSettingsFeature(this IServiceCollection services)
    {
        services.AddTransient<SettingsViewModel>();
        return services;
    }
}
