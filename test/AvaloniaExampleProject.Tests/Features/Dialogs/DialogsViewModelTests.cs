using Avalonia.Headless.XUnit;
using AvaloniaExampleProject.Features.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using static AvaloniaExampleProject.Tests.VerifyHelpers;

namespace AvaloniaExampleProject.Tests.Features.Dialogs;

public sealed class DialogsViewModelTests
{
    [AvaloniaFact]
    public Task Render()
    {
        var control = new DialogsView
        {
            ViewModel = ActivatorUtilities.CreateInstance<DialogsViewModel>(TestAppBuilder.Services),
        };
        return VerifyControl(control);
    }

    [Fact]
    public void ProjectDialogResult_IsOnlyAvailable_WhenRequiredFieldsAreComplete()
    {
        var viewModel = new ProjectDialogViewModel();

        viewModel.TryGetResultData(out ProjectDialogResult? incompleteResult).ShouldBeFalse();
        incompleteResult.ShouldBeNull();

        viewModel.ProjectName = "Telemetry App";

        viewModel.TryGetResultData(out ProjectDialogResult? result).ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Telemetry App");
        result.Template.ShouldBe("Blank");
        result.OpenAfterCreate.ShouldBeTrue();
    }
}
