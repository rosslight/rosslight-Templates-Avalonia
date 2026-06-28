using System.Text.Json.Serialization;

namespace AvaloniaExampleProject;

[JsonSerializable(typeof(MainConfig))]
public sealed partial class JsonContext : JsonSerializerContext;
