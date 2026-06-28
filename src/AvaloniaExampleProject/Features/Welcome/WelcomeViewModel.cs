using AvaloniaExampleProject.Assets;
using AvaloniaExampleProject.Shell;
using CommunityToolkit.Mvvm.Input;
using Darp.Utils.Dialog;

namespace AvaloniaExampleProject.Features.Welcome;

public sealed partial class WelcomeViewModel(Resources i18N, IDialogService dialogService) : ViewModelBase
{
    private readonly IDialogService _dialogService = dialogService;
    public Resources I18N { get; } = i18N;

    [RelayCommand]
    private async Task ShowInputDialog(CancellationToken cancellationToken)
    {
        var result = await _dialogService
            .CreateInputDialog(I18N.Welcome_ClickMe_InputTitle, I18N.Welcome_ClickMe_InputMessage)
            .ShowAsync(cancellationToken);
        if (!result.IsPrimary || !result.TryGetResultData(out string? resultData))
            return;
        await _dialogService.ShowMessageBoxDialogAsync(
            I18N.Welcome_ClickMe_ResultsTitle,
            resultData,
            cancellationToken
        );
    }
}
