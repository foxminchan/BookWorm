using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookWorm.Chassis.Utilities.Converters;

public sealed class DecimalJsonConverter : JsonConverter<decimal>
{
    public static DecimalJsonConverter Instance { get; } = new();

    public override decimal Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return decimal.Round(reader.GetDecimal(), 2);
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(decimal.Round(value, 2));
    }
}
