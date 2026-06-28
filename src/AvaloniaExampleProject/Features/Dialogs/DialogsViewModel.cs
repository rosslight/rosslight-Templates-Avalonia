using System.ComponentModel.DataAnnotations;
using Avalonia.Media;
using AvaloniaExampleProject.Assets;
using AvaloniaExampleProject.Shell;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Darp.Utils.Dialog;
using Darp.Utils.Dialog.DialogData;
using FluentAvalonia.UI.Controls;

namespace AvaloniaExampleProject.Features.Dialogs;

public sealed partial class DialogsViewModel : ViewModelBase
{
    private static readonly TimeSpan AutoCloseDelay = TimeSpan.FromSeconds(5);
    private readonly IDialogService _dialogService;
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;

    public DialogsViewModel(Resources i18N, IDialogService dialogService)
        : this(i18N, dialogService, Task.Delay) { }

    internal DialogsViewModel(
        Resources i18N,
        IDialogService dialogService,
        Func<TimeSpan, CancellationToken, Task> delay
    )
    {
        I18N = i18N;
        _dialogService = dialogService;
        _delay = delay;

        LastResult = I18N.Dialogs_Result_Empty;
        ShowcaseItems =
        [
            CreateItem(
                I18N.Dialogs_MessageBox_Title,
                I18N.Dialogs_MessageBox_Description,
                FASymbol.Message,
                "#0F78D4",
                I18N.Dialogs_OpenButton,
                ShowMessageBoxDialogCommand
            ),
            CreateItem(
                I18N.Dialogs_SelectableMessage_Title,
                I18N.Dialogs_SelectableMessage_Description,
                FASymbol.Clipboard,
                "#6B5DD3",
                I18N.Dialogs_OpenButton,
                ShowSelectableMessageBoxDialogCommand
            ),
            CreateItem(
                I18N.Dialogs_Input_Title,
                I18N.Dialogs_Input_Description,
                FASymbol.Edit,
                "#C95F00",
                I18N.Dialogs_OpenButton,
                ShowInputDialogCommand
            ),
            CreateItem(
                I18N.Dialogs_UsernamePassword_Title,
                I18N.Dialogs_UsernamePassword_Description,
                FASymbol.ContactInfo,
                "#238A3B",
                I18N.Dialogs_OpenButton,
                ShowUsernamePasswordDialogCommand
            ),
            CreateItem(
                I18N.Dialogs_CustomInput_Title,
                I18N.Dialogs_CustomInput_Description,
                FASymbol.Page,
                "#D16F00",
                I18N.Dialogs_OpenButton,
                ShowCustomInputDialogCommand
            ),
            CreateItem(
                I18N.Dialogs_SecondaryAction_Title,
                I18N.Dialogs_SecondaryAction_Description,
                FASymbol.SaveAs,
                "#476F00",
                I18N.Dialogs_OpenButton,
                ShowSecondaryActionDialogCommand
            ),
            CreateItem(
                I18N.Dialogs_IsEnabled_Title,
                I18N.Dialogs_IsEnabled_Description,
                FASymbol.Accept,
                "#008272",
                I18N.Dialogs_OpenButton,
                ShowIsEnabledDialogCommand
            ),
            CreateItem(
                I18N.Dialogs_SyncValidation_Title,
                I18N.Dialogs_SyncValidation_Description,
                FASymbol.Permissions,
                "#C19C00",
                I18N.Dialogs_OpenButton,
                ShowSyncValidationDialogCommand
            ),
            CreateItem(
                I18N.Dialogs_AsyncValidation_Title,
                I18N.Dialogs_AsyncValidation_Description,
                FASymbol.Sync,
                "#7657C8",
                I18N.Dialogs_OpenButton,
                ShowAsyncValidationDialogCommand
            ),
            CreateItem(
                I18N.Dialogs_AutoClose_Title,
                I18N.Dialogs_AutoClose_Description,
                FASymbol.Clock,
                "#2B7CD3",
                I18N.Dialogs_OpenButton,
                ShowAutoCloseDialogCommand
            ),
            CreateItem(
                I18N.Dialogs_ForcedDecision_Title,
                I18N.Dialogs_ForcedDecision_Description,
                FASymbol.Cancel,
                "#D83B01",
                I18N.Dialogs_OpenButton,
                ShowForcedDecisionDialogCommand
            ),
        ];
    }

    public Resources I18N { get; }
    public IReadOnlyList<DialogShowcaseItem> ShowcaseItems { get; }

    [ObservableProperty]
    public partial string LastResult { get; private set; } = string.Empty;

    [RelayCommand]
    private async Task ShowMessageBoxDialog(CancellationToken cancellationToken)
    {
        var result = await _dialogService
            .CreateMessageBoxDialog(I18N.Dialogs_MessageBox_DialogTitle, I18N.Dialogs_MessageBox_DialogMessage)
            .ShowAsync(cancellationToken);

        LastResult = result.IsPrimary ? I18N.Dialogs_Result_MessageAcknowledged : I18N.Dialogs_Result_Dismissed;
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

        LastResult = result.IsPrimary ? I18N.Dialogs_Result_SelectableMessageClosed : I18N.Dialogs_Result_Dismissed;
    }

    [RelayCommand]
    private async Task ShowInputDialog(CancellationToken cancellationToken)
    {
        var result = await _dialogService
            .CreateInputDialog(I18N.Dialogs_Input_DialogTitle, I18N.Dialogs_Input_DialogMessage)
            .ConfigureInput(I18N.Dialogs_Input_Watermark, validateInput: ValidateDisplayName)
            .ShowAsync(cancellationToken);

        LastResult =
            result.IsPrimary && result.TryGetResultData(out string? displayName)
                ? I18N.FormatDialogs_Result_DisplayName(displayName)
                : I18N.Dialogs_Result_Dismissed;
    }

    [RelayCommand]
    private async Task ShowUsernamePasswordDialog(CancellationToken cancellationToken)
    {
        var result = await _dialogService
            .CreateUsernamePasswordDialog(I18N.Dialogs_UsernamePassword_DialogTitle)
            .ConfigureUsernameStep(I18N.Dialogs_UsernamePassword_UsernameMessage, ValidateEmail)
            .ConfigurePasswordStep(I18N.Dialogs_UsernamePassword_PasswordMessage, ValidatePassword)
            .ShowAsync(cancellationToken);

        LastResult =
            result.IsPrimary && result.TryGetResultData(out UsernamePasswordData? data)
                ? I18N.FormatDialogs_Result_SignedIn(data.Username)
                : I18N.Dialogs_Result_Dismissed;
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
        {
            LastResult = I18N.Dialogs_Result_Dismissed;
            return;
        }

        LastResult = result.IsSecondary
            ? I18N.FormatDialogs_Result_ProjectDraftSaved(project.Name)
            : I18N.FormatDialogs_Result_ProjectSaved(project.Name);
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
        using var dialog = _dialogService
            .CreateMessageBoxDialog(I18N.Dialogs_AutoClose_DialogTitle, I18N.Dialogs_AutoClose_DialogMessage)
            .ShowAsync(cancellationToken);
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, dialog.Token);

        try
        {
            await _delay(AutoCloseDelay, linkedSource.Token);
            dialog.Dispose();
            LastResult = I18N.Dialogs_Result_ClosedByDisposal;
        }
        catch (OperationCanceledException) when (dialog.IsClosed && !cancellationToken.IsCancellationRequested)
        {
            LastResult = I18N.Dialogs_Result_Dismissed;
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

        LastResult = result.IsPrimary ? I18N.Dialogs_Result_DeleteConfirmed : I18N.Dialogs_Result_Dismissed;
    }

    private void SetProjectResult(ContentDialogResult<ProjectDialogViewModel> result, string prefix)
    {
        LastResult =
            result.IsPrimary && result.TryGetResultData(out ProjectDialogResult? project)
                ? I18N.FormatDialogs_Result_Project(prefix, project.Name, project.Template, project.OpenAfterCreate)
                : I18N.Dialogs_Result_Dismissed;
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
        await _delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        if (!string.Equals(content.ProjectName?.Trim(), "taken", StringComparison.OrdinalIgnoreCase))
            return true;

        content.ErrorMessage = I18N.Dialogs_AsyncValidation_NameTakenError;
        return false;
    }

    private static IObservable<bool> CreateEnabledObservable(ProjectDialogViewModel content) =>
        new PropertyChangedObservable<ProjectDialogViewModel>(
            content,
            nameof(ProjectDialogViewModel.IsComplete),
            x => x.IsComplete
        );

    private static DialogShowcaseItem CreateItem(
        string title,
        string description,
        FASymbol icon,
        string iconColor,
        string buttonText,
        IAsyncRelayCommand command
    ) => new(title, description, icon, new SolidColorBrush(Color.Parse(iconColor)), buttonText, command);

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
