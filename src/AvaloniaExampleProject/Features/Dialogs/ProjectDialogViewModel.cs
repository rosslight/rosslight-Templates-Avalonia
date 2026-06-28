using System.Diagnostics.CodeAnalysis;
using AvaloniaExampleProject.Assets;
using AvaloniaExampleProject.Shell;
using CommunityToolkit.Mvvm.ComponentModel;
using Darp.Utils.Dialog.DialogData;

namespace AvaloniaExampleProject.Features.Dialogs;

public sealed partial class ProjectDialogViewModel(Resources? i18N = null)
    : ViewModelBase,
        IDialogData<ProjectDialogResult>
{
    public static IReadOnlyList<string> AvailableTemplates { get; } = ["Blank", "Console", "Desktop"];
    public Resources I18N { get; } = i18N ?? Resources.Default;
    public IReadOnlyList<string> TemplateOptions => AvailableTemplates;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsComplete))]
    public partial string? ProjectName { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsComplete))]
    public partial string? SelectedTemplate { get; set; } = AvailableTemplates[0];

    [ObservableProperty]
    public partial bool OpenAfterCreate { get; set; } = true;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public bool IsComplete => !string.IsNullOrWhiteSpace(ProjectName) && !string.IsNullOrWhiteSpace(SelectedTemplate);

    public bool TryGetResultData([NotNullWhen(true)] out ProjectDialogResult? resultData)
    {
        if (!IsComplete)
        {
            resultData = null;
            return false;
        }

        resultData = new ProjectDialogResult(ProjectName!.Trim(), SelectedTemplate!, OpenAfterCreate);
        return true;
    }
}
