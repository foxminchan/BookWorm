using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookWorm.Chassis.Utilities.Converters;

public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public static DateOnlyJsonConverter Instance { get; } = new();

    public override DateOnly Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return DateOnly.TryParseExact(
            reader.GetString(),
            Format,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date
        )
            ? date
            : throw new JsonException($"Invalid date format. Expected format: {Format}");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("O", CultureInfo.InvariantCulture));
    }
}
