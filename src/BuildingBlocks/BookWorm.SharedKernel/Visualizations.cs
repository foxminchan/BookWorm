using System.ComponentModel;
using System.Text.Json.Serialization;

namespace BookWorm.SharedKernel;

[JsonConverter(typeof(JsonStringEnumConverter<Visualizations>))]
public enum Visualizations : byte
{
    [Description("Mermaid")]
    Mermaid = 1,

    [Description("DiGraph")]
    Dot = 2,
}
