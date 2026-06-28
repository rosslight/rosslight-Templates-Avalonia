namespace AvaloniaExampleProject.Features.Settings;

public sealed record ThemeOption(string Key, IObservable<string> DisplayName);
