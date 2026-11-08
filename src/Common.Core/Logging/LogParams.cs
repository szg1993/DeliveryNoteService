using Medusa.Enap.Common.Core.Utils;
using System.Runtime.CompilerServices;

namespace Medusa.Enap.Common.Core.Logging;

public class LogParams
{
    private readonly Dictionary<string, object?> _dictionary = [];

    // Must not derive from Dictionary, otherwise the framework will not add it to the log context
    // I suppose because it is a custom type.
    public static implicit operator Dictionary<string, object?>(LogParams d) => d._dictionary;

    public static LogParams CreateWithParam(object? paramValue, [CallerArgumentExpression(nameof(paramValue))] string? paramName = null)
        => Create(paramName!, paramValue);

    public static LogParams Create(string paramName, object? value)
    {
        ArgumentNullException.ThrowIfNull(paramName);
        return new LogParams().Add(paramName, value);
    }

    /// <summary>
    /// Uses the variable name passed for paramValue.
    /// </summary>
    /// <param name="paramValue"></param>
    /// <param name="paramName"></param>
    /// <returns></returns>
    public LogParams AddParam(object? paramValue, [CallerArgumentExpression(nameof(paramValue))] string? paramName = null)
        => Add(paramName!, paramValue);

    public LogParams Add(LogParams logParams)
        => Add((Dictionary<string, object?>)logParams);

    public LogParams Add(IEnumerable<KeyValuePair<string, object?>> logParams)
    {
        foreach (var p in logParams)
            Add(p.Key, p.Value);

        return this;
    }

    public LogParams Add(string paramName, object? paramValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paramName);

        paramName = paramName
            .Split('.')[^1]
            .TrimStart('_')
            .ToCamelCase();

        _dictionary.Add(paramName, paramValue);
        return this;
    }

    public void Clear() => _dictionary.Clear();
}
