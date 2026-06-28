using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;

namespace AvaloniaExampleProject.Features.Dialogs;

public sealed record DialogShowcaseItem(
    string Title,
    string Description,
    FASymbol Icon,
    IBrush IconBrush,
    string ButtonText,
    IAsyncRelayCommand Command
);
