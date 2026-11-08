namespace Medusa.Enap.Common.Core.Integration;

public class CorrelationIdAccessor
{
    private string _correlationId = Guid.NewGuid().ToString();

    public string CorrelationId
    {
        get => _correlationId;
        set => _correlationId = IsValid(value) ? value : _correlationId;
    }

    private static bool IsValid(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && value != Guid.Empty.ToString();
    }
}
