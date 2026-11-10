using System.Buffers;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeliveryNoteService.Core.UseCases.Notes;

public sealed class ExactNoteDecimalConverter : JsonConverter<decimal?>
{
    public override decimal? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.Number)
            throw new JsonException("An exactly representable decimal number is required.");

        var input = reader.HasValueSequence
            ? Encoding.UTF8.GetString(reader.ValueSequence.ToArray())
            : Encoding.UTF8.GetString(reader.ValueSpan);
        var original = Normalize(input);
        if (original.Digits.Length == 0)
            return decimal.Zero;
        if (!reader.TryGetDecimal(out var value) ||
            original != Normalize(value.ToString(CultureInfo.InvariantCulture))
        )
        {
            throw new JsonException("The decimal number cannot be represented without rounding.");
        }

        return value;
    }

    public override void Write(
        Utf8JsonWriter writer,
        decimal? value,
        JsonSerializerOptions options
    )
    {
        if (value.HasValue)
            writer.WriteNumberValue(value.Value);
        else
            writer.WriteNullValue();
    }

    private static (string Digits, BigInteger Exponent, bool Negative) Normalize(string number)
    {
        var exponentIndex = number.IndexOfAny(['e', 'E']);
        var mantissa = exponentIndex < 0 ? number : number[..exponentIndex];
        var decimalPoint = mantissa.IndexOf('.');
        var fractionalDigits = decimalPoint < 0 ? 0 : mantissa.Length - decimalPoint - 1;
        var digits = mantissa.TrimStart('-').Replace(".", "").TrimStart('0');
        var significantDigits = digits.TrimEnd('0');
        if (significantDigits.Length == 0)
            return ("", BigInteger.Zero, false);

        var exponent = exponentIndex < 0
            ? BigInteger.Zero
            : BigInteger.Parse(number[(exponentIndex + 1)..], CultureInfo.InvariantCulture);
        return (
            significantDigits,
            exponent - fractionalDigits + digits.Length - significantDigits.Length,
            number[0] == '-'
        );
    }
}