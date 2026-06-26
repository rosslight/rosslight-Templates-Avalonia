using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace AvaloniaExampleProject.Desktop;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            // We are allowed to use Log.Logger here because we have set it in Bootstrapper.AddSerilogLogging
            Log.Logger.Debug("AvaloniaExampleProject stopped!");
            Log.CloseAndFlush();
        }
        catch (Exception e)
        {
            Log.Logger.Fatal(e, "AvaloniaExampleProject terminated unexpectedly: {Message}", e.Message);
            Log.CloseAndFlush();
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        var provider = new ServiceCollection().AddAppServices().AddSerilogLogging().BuildServiceProvider();
        // Set the global logger here to avoid surprise later. Prefer to use an injected logger!
        Log.Logger = provider.GetRequiredService<ILogger>();
        return AppBuilder.Configure(() => new App(provider)).UsePlatformDetect().WithInterFont().LogToTrace();
    }
}
