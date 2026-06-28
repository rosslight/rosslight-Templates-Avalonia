using Avalonia.Headless.XUnit;
using AvaloniaExampleProject.Features.Welcome;
using Microsoft.Extensions.DependencyInjection;
using static AvaloniaExampleProject.Tests.VerifyHelpers;

namespace AvaloniaExampleProject.Tests.Features.Welcome;

public sealed class WelcomeViewModelTests
{
    [AvaloniaFact]
    public Task Render()
    {
        var viewModel = TestAppBuilder.Services.GetRequiredService<WelcomeViewModel>();
        var control = new WelcomeView { ViewModel = viewModel };
        return VerifyControl(control);
    }
}
