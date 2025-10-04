using System.ComponentModel;
using System.Text.Json.Serialization;

namespace BookWorm.SharedKernel;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VisualizationType : byte
{
    [Description("Mermaid")]
    Mermaid = 0,

    [Description("DiGraph")]
    Dot = 1,
}
