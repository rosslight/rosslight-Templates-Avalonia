using System.Text.Json.Serialization;
using Serilog.Events;

namespace AvaloniaExampleProject;

// Configs are assumed to be immutable record objects. When using together with Darp.Configuration make sure they are equatable
//
// Warning: Source-generated JSON serialization can behave differently than reflection-based serialization!
// To ensure that this works, follow:
// - Mark constructors as [JsonConstructor]
// - Default values should be provided as constructor parameters
// - If you have to initialize an object, accept <null> inside the constructor and fall back when initializing the property

[method: JsonConstructor]
public sealed record MainConfig(UserPreferencesConfig? UserPreferences = null, DiagnosticsConfig? Diagnostics = null)
{
    public MainConfig()
        : this(UserPreferences: null) { }

    public UserPreferencesConfig UserPreferences { get; init; } = UserPreferences ?? new UserPreferencesConfig();
    public DiagnosticsConfig Diagnostics { get; init; } = Diagnostics ?? new DiagnosticsConfig();
}

[method: JsonConstructor]
public sealed record UserPreferencesConfig(string SelectedLanguage = "en", string SelectedTheme = "Default");

[method: JsonConstructor]
public sealed record DiagnosticsConfig(LogEventLevel LogLevel = LogEventLevel.Information);
