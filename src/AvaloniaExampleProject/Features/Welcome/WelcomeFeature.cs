using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaExampleProject.Features.Welcome;

public static class WelcomeFeature
{
    public static IServiceCollection AddWelcomeFeature(this IServiceCollection services)
    {
        services.AddTransient<WelcomeViewModel>();
        return services;
    }
}
