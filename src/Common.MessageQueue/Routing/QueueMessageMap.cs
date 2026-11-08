namespace Medusa.Enap.Common.MessageQueue.Routing;

public class QueueMessageMap
{
    private sealed record Mapping(Type MessageType, Func<string> GetQueueName);

    private List<Mapping> _mappings { get; } = [];

    public void Map<TMessage>(Func<string> queueNameSelector)
    {
        _mappings.Add(new Mapping(typeof(TMessage), queueNameSelector));
    }

    public (Type msgType, string queueName)[] GetAll()
        => [.. _mappings.Select(m => (m.MessageType, m.GetQueueName()))];

    public string GetQueueName<TMessage>()
    {
        return GetQueueName(typeof(TMessage));
    }

    public string GetQueueName(Type messageType)
    {
        ArgumentNullException.ThrowIfNull(messageType);

        return _mappings
            .First(mapping => mapping.MessageType == messageType)
            .GetQueueName();
    }
}
