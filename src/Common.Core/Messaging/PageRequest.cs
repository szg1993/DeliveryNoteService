namespace Medusa.Enap.Common.Core.Messaging;

public record PageRequest(int Page, int Limit)
{
    public static PageRequest Default { get; } = new(1, 10);
    public static PageRequest Unpaged { get; } = new(1, 0);
}
