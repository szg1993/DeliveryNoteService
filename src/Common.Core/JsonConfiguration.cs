using Common.Core.Utils;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Common.Core;

public static class JsonConfiguration
{
    private static JsonSerializerOptions? _jsonOpts;

    public static JsonSerializerOptions GetOptions()
        => _jsonOpts ??= Configure(new JsonSerializerOptions(JsonSerializerDefaults.Web));

    public static JsonSerializerOptions Configure(JsonSerializerOptions opt)
    {
        opt.PropertyNameCaseInsensitive = true;
        opt.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opt.Converters.Add(new JsonStringEnumConverter());
        opt.Converters.Add(new DateOnlyConverter());
        opt.Converters.Add(new NullableDateOnlyConverter());
        opt.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        SetDebugOptions(opt);
        opt.TypeInfoResolver = new PolymorphicTypeResolver();
        return opt;
    }

    [Conditional("DEBUG")]
    public static void SetDebugOptions(JsonSerializerOptions opt)
    {
        opt.WriteIndented = true;
    }
}

file sealed class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    public PolymorphicTypeResolver()
    {
        Modifiers.Add(ConfigurePolymorphism);
    }

    private void ConfigurePolymorphism(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type.IsAbstract && typeInfo.Type.IsClass && typeInfo.Type.Assembly.GetName().Name?.Contains("Medusa") == true)
        {
            typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$type",
                IgnoreUnrecognizedTypeDiscriminators = true
            };

            foreach (var derivedType in GetDerivedTypes(typeInfo.Type))
            {
                string discriminator = derivedType.Name;
                typeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derivedType, discriminator));
            }
        }
    }

    private static IEnumerable<Type> GetDerivedTypes(Type baseType)
    {
        return AssemblyUtils.GetMedusaAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t));
    }
}

public class DateOnlyConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new NullableDateOnlyConverter().Read(ref reader, typeToConvert, options)
            ?? DateOnly.MinValue;

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        => new NullableDateOnlyConverter().Write(writer, value, options);
}

public class NullableDateOnlyConverter : JsonConverter<DateOnly?>
{
    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return DateTimeOffset
                .FromUnixTimeSeconds(reader.GetInt64())
                .ToHungarianTime()
                .ToDateOnly();

        if (reader.TokenType == JsonTokenType.String)
        {
            var dateText = reader.GetString();
            return string.IsNullOrWhiteSpace(dateText) ? null : dateText.ParseDateOnly();
        }

        if (reader.TokenType == JsonTokenType.Null)
            return null;

        throw new JsonException("Invalid token type for DateOnly conversion.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        => writer.WriteStringValue(value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}