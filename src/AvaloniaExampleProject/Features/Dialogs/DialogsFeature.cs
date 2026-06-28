using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaExampleProject.Features.Dialogs;

public static class DialogsFeature
{
    public static IServiceCollection AddDialogsFeature(this IServiceCollection services) =>
        services.AddTransient<DialogsViewModel>();
}
