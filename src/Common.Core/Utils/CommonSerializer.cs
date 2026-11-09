using Common.Core.Exceptions;
using System.Text.Json;

namespace Common.Core.Utils;

public static class CommonSerializer
{
    public static string ToJson<T>(T obj)
        => JsonSerializer.Serialize(obj, JsonConfiguration.GetOptions());

    public static T? FromJson<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonConfiguration.GetOptions());
        }
        catch (Exception ex)
        {
            throw new DeserializationException(ex);
        }
    }

    public static object? FromJson(Type msgType, string json)
    {
        try
        {
            return JsonSerializer.Deserialize(json, msgType, JsonConfiguration.GetOptions());
        }
        catch (Exception ex)
        {
            throw new DeserializationException(ex);
        }
    }
}