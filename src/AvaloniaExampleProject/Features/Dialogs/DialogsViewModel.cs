using System.ComponentModel.DataAnnotations;
using AvaloniaExampleProject.Assets;
using AvaloniaExampleProject.Services;
using AvaloniaExampleProject.Shell;
using CommunityToolkit.Mvvm.Input;
using Darp.Utils.Dialog;
using Darp.Utils.Dialog.DialogData;
using DynamicData.Binding;

namespace AvaloniaExampleProject.Features.Dialogs;

public sealed partial class DialogsViewModel : ViewModelBase
{
    private static readonly TimeSpan s_autoCloseDelay = TimeSpan.FromSeconds(5);
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;

    public DialogsViewModel(Resources i18N, IDialogService dialogService, INotificationService notificationService)
    {
        I18N = i18N;
        _dialogService = dialogService;
        _notificationService = notificationService;
    }

    public Resources I18N { get; }

    [RelayCommand]
    private async Task ShowMessageBoxDialog(CancellationToken cancellationToken)
    {
        var result = await _dialogService
            .CreateMessageBoxDialog(I18N.Dialogs_MessageBox_DialogTitle, I18N.Dialogs_MessageBox_DialogMessage)
            .ShowAsync(cancellationToken);

        if (result.IsPrimary)
            ShowResultNotification(I18N.Dialogs_Result_MessageAcknowledged);
    }

    [RelayCommand]
    private async Task ShowSelectableMessageBoxDialog(CancellationToken cancellationToken)
    {
        var result = await _dialogService
            .CreateMessageBoxDialog(
                I18N.Dialogs_SelectableMessage_DialogTitle,
                I18N.Dialogs_SelectableMessage_DialogMessage,
                true
            )
            .ShowAsync(cancellationToken);

        if (result.IsPrimary)
            ShowResultNotification(I18N.Dialogs_Result_SelectableMessageClosed);
    }

    [RelayCommand]
    private async Task ShowInputDialog(CancellationToken cancellationToken)
    {
        var result = await _dialogService
            .CreateInputDialog(I18N.Dialogs_Input_DialogTitle, I18N.Dialogs_Input_DialogMessage)
            .ConfigureInput(I18N.Dialogs_Input_Watermark, validateInput: ValidateDisplayName)
            .ShowAsync(cancellationToken);

        if (result.IsPrimary && result.TryGetResultData(out string? displayName))
            ShowResultNotification(I18N.FormatDialogs_Result_DisplayName(displayName));
    }

    [RelayCommand]
    private async Task ShowUsernamePasswordDialog(CancellationToken cancellationToken)
    {
        var result = await _dialogService
            .CreateUsernamePasswordDialog(I18N.Dialogs_UsernamePassword_DialogTitle)
            .ConfigureUsernameStep(I18N.Dialogs_UsernamePassword_UsernameMessage, ValidateEmail)
            .ConfigurePasswordStep(I18N.Dialogs_UsernamePassword_PasswordMessage, ValidatePassword)
            .ShowAsync(cancellationToken);

        if (result.IsPrimary && result.TryGetResultData(out UsernamePasswordData? data))
            ShowResultNotification(I18N.FormatDialogs_Result_SignedIn(data.Username));
    }

    [RelayCommand]
    private async Task ShowCustomInputDialog(CancellationToken cancellationToken)
    {
        var content = new ProjectDialogViewModel(I18N);
        var result = await _dialogService
            .CreateContentDialog(I18N.Dialogs_CustomInput_DialogTitle, content)
            .SetDefaultButton(ContentDialogButton.Primary)
            .SetCloseButton(I18N.Dialogs_CancelButton)
            .SetPrimaryButton(I18N.Dialogs_CreateButton, CreateEnabledObservable(content))
            .ShowAsync(cancellationToken);

        SetProjectResult(result, I18N.Dialogs_Result_ProjectCreated);
    }

    [RelayCommand]
    private async Task ShowSecondaryActionDialog(CancellationToken cancellationToken)
    {
        var content = new ProjectDialogViewModel(I18N);
        var result = await _dialogService
            .CreateContentDialog(I18N.Dialogs_SecondaryAction_DialogTitle, content)
            .SetDefaultButton(ContentDialogButton.Primary)
            .SetCloseButton(I18N.Dialogs_CancelButton)
            .SetPrimaryButton(I18N.Dialogs_SaveButton, CreateEnabledObservable(content))
            .SetSecondaryButton(I18N.Dialogs_SaveDraftButton, CreateEnabledObservable(content))
            .ShowAsync(cancellationToken);

        if (!result.TryGetResultData(out ProjectDialogResult? project))
            return;

        ShowResultNotification(
            result.IsSecondary
                ? I18N.FormatDialogs_Result_ProjectDraftSaved(project.Name)
                : I18N.FormatDialogs_Result_ProjectSaved(project.Name)
        );
    }

    [RelayCommand]
    private async Task ShowIsEnabledDialog(CancellationToken cancellationToken)
    {
        var content = new ProjectDialogViewModel(I18N) { SelectedTemplate = null };
        var result = await _dialogService
            .CreateContentDialog(I18N.Dialogs_IsEnabled_DialogTitle, content)
            .SetDefaultButton(ContentDialogButton.Primary)
            .SetCloseButton(I18N.Dialogs_CancelButton)
            .SetPrimaryButton(I18N.Dialogs_CreateButton, CreateEnabledObservable(content))
            .ShowAsync(cancellationToken);

        SetProjectResult(result, I18N.Dialogs_Result_ProjectCreated);
    }

    [RelayCommand]
    private async Task ShowSyncValidationDialog(CancellationToken cancellationToken)
    {
        var content = new ProjectDialogViewModel(I18N);
        var result = await _dialogService
            .CreateContentDialog(I18N.Dialogs_SyncValidation_DialogTitle, content)
            .SetDefaultButton(ContentDialogButton.Primary)
            .SetCloseButton(I18N.Dialogs_CancelButton)
            .SetPrimaryButton(I18N.Dialogs_CreateButton, CreateEnabledObservable(content), ValidateReservedProjectName)
            .ShowAsync(cancellationToken);

        SetProjectResult(result, I18N.Dialogs_Result_ProjectCreated);
    }

    [RelayCommand]
    private async Task ShowAsyncValidationDialog(CancellationToken cancellationToken)
    {
        var content = new ProjectDialogViewModel(I18N);
        var result = await _dialogService
            .CreateContentDialog(I18N.Dialogs_AsyncValidation_DialogTitle, content)
            .SetDefaultButton(ContentDialogButton.Primary)
            .SetCloseButton(I18N.Dialogs_CancelButton)
            .SetPrimaryButton(
                I18N.Dialogs_CreateButton,
                CreateEnabledObservable(content),
                ValidateWorkspaceAvailableAsync
            )
            .ShowAsync(cancellationToken);

        SetProjectResult(result, I18N.Dialogs_Result_ProjectCreated);
    }

    [RelayCommand]
    private async Task ShowAutoCloseDialog(CancellationToken cancellationToken)
    {
        DialogAwaitable<MessageBoxViewModel> dialog = _dialogService
            .CreateMessageBoxDialog(I18N.Dialogs_AutoClose_DialogTitle, I18N.Dialogs_AutoClose_DialogMessage)
            .ShowAsync(cancellationToken);

        try
        {
            await Task.Delay(s_autoCloseDelay, dialog.Token);
            ShowResultNotification(I18N.Dialogs_Result_ClosedByDisposal);
        }
        catch (OperationCanceledException)
        { }
        finally
        {
            dialog.Dispose();
        }
    }

    [RelayCommand]
    private async Task ShowForcedDecisionDialog(CancellationToken cancellationToken)
    {
        var result = await _dialogService
            .CreateMessageBoxDialog(I18N.Dialogs_ForcedDecision_DialogTitle, I18N.Dialogs_ForcedDecision_DialogMessage)
            .SetClosingOnEscape(false)
            .SetCloseButton(I18N.Dialogs_CancelButton)
            .SetPrimaryButton(I18N.Dialogs_DeleteButton)
            .ShowAsync(cancellationToken);

        if (result.IsPrimary)
            ShowResultNotification(I18N.Dialogs_Result_DeleteConfirmed);
    }

    private void SetProjectResult(ContentDialogResult<ProjectDialogViewModel> result, string prefix)
    {
        if (result.IsPrimary && result.TryGetResultData(out ProjectDialogResult? project))
            ShowResultNotification(
                I18N.FormatDialogs_Result_Project(prefix, project.Name, project.Template, project.OpenAfterCreate)
            );
    }

    private void ShowResultNotification(string result)
    {
        _notificationService.ShowInformation(I18N.Dialogs_Result_Title, result);
    }

    private bool ValidateReservedProjectName(ProjectDialogViewModel content)
    {
        if (!string.Equals(content.ProjectName?.Trim(), "admin", StringComparison.OrdinalIgnoreCase))
            return true;

        content.ErrorMessage = I18N.Dialogs_SyncValidation_ReservedNameError;
        return false;
    }

    private async Task<bool> ValidateWorkspaceAvailableAsync(
        ProjectDialogViewModel content,
        CancellationToken cancellationToken
    )
    {
        await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        if (!string.Equals(content.ProjectName?.Trim(), "taken", StringComparison.OrdinalIgnoreCase))
            return true;

        content.ErrorMessage = I18N.Dialogs_AsyncValidation_NameTakenError;
        return false;
    }

    private static IObservable<bool> CreateEnabledObservable(ProjectDialogViewModel content) =>
        content.WhenValueChanged(x => x.IsComplete);

    private ValidationResult? ValidateDisplayName(string? input) =>
        string.IsNullOrWhiteSpace(input)
            ? new ValidationResult(I18N.Dialogs_Input_RequiredError)
            : ValidationResult.Success;

    private ValidationResult? ValidateEmail(string username) =>
        username.Contains('@', StringComparison.Ordinal)
            ? ValidationResult.Success
            : new ValidationResult(I18N.Dialogs_UsernamePassword_EmailError);

    private ValidationResult? ValidatePassword(string password) =>
        password.Length >= 8
            ? ValidationResult.Success
            : new ValidationResult(I18N.Dialogs_UsernamePassword_PasswordError);
}
