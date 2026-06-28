using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;

namespace AvaloniaExampleProject.Features.Dialogs;

public sealed class DialogShowcaseCard : TemplatedControl
{
    public static readonly StyledProperty<string?> TitleProperty = AvaloniaProperty.Register<
        DialogShowcaseCard,
        string?
    >(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty = AvaloniaProperty.Register<
        DialogShowcaseCard,
        string?
    >(nameof(Description));

    public static readonly StyledProperty<FASymbol> IconProperty = AvaloniaProperty.Register<
        DialogShowcaseCard,
        FASymbol
    >(nameof(Icon));

    public static readonly StyledProperty<IBrush?> IconBrushProperty = AvaloniaProperty.Register<
        DialogShowcaseCard,
        IBrush?
    >(nameof(IconBrush));

    public static readonly StyledProperty<string?> ButtonTextProperty = AvaloniaProperty.Register<
        DialogShowcaseCard,
        string?
    >(nameof(ButtonText));

    public static readonly StyledProperty<ICommand?> CommandProperty = AvaloniaProperty.Register<
        DialogShowcaseCard,
        ICommand?
    >(nameof(Command));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public FASymbol Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }

    public string? ButtonText
    {
        get => GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
}
