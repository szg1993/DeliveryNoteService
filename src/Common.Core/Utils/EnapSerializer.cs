using Medusa.Enap.Common.Core.Configuration;
using Medusa.Enap.Common.Core.Exceptions;
using System.Text.Json;

namespace Medusa.Enap.Common.Core.Utils;

public static class EnapSerializer
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
