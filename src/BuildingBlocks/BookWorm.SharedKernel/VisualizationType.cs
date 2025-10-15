using System.ComponentModel;
using System.Text.Json.Serialization;

namespace BookWorm.SharedKernel;

[Flags, JsonConverter(typeof(JsonStringEnumConverter<VisualizationType>))]
public enum VisualizationType : byte
{
    [Description("Mermaid")]
    Mermaid = 0,

    [Description("DiGraph")]
    Dot = 1,
}
