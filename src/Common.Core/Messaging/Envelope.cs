namespace Medusa.Enap.Common.Core.Messaging;

public class Envelope<T>(T content)
{
    public T Content { get; set; } = content;
}
