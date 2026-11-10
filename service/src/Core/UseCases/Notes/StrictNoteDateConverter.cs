using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeliveryNoteService.Core.UseCases.Notes;

public sealed class StrictNoteDateConverter : JsonConverter<DateOnly?>
{
    public override bool HandleNull => true;

    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String ||
            !DateOnly.TryParseExact(
                reader.GetString(),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date
            )
        )
        {
            throw new JsonException("A calendar date in yyyy-MM-dd format is required.");
        }

        return date;
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }
}