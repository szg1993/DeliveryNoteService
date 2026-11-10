using System.Reflection;

namespace Common.Testing;

public static class ErrorCodeCollector
{
    public static List<string> Run(Type type)
    {
        var errorCodes = new List<string>();

        errorCodes.AddRange(
            type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi =>
                    fi is { IsLiteral: true, IsInitOnly: false } &&
                    fi.FieldType == typeof(string) &&
                    fi.Name != "Prefix")
                .Select(fi => (string)fi.GetRawConstantValue()!)
        );

        var nestedTypes = type.GetNestedTypes(BindingFlags.Public)
            .Where(t => t.IsAbstract && t.IsSealed);

        foreach (var nestedType in nestedTypes)
            errorCodes.AddRange(Run(nestedType));

        return errorCodes;
    }
}