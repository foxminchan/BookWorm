using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookWorm.Chassis.Converters;

public sealed class StringTrimmerJsonConverter : JsonConverter<string>
{
    public static StringTrimmerJsonConverter Instance { get; } = new();

    public override string? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return reader.GetString()?.Trim();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
