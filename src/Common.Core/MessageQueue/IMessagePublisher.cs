namespace Common.Core.MessageQueue;

public interface IMessagePublisher
{
    Task PublishAsync(object message);

    Task PublishAsync(IEnumerable<object> messages);
}