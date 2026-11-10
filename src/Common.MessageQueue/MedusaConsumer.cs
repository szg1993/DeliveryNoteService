
namespace Common.MessageQueue;

public interface IMedusaConsumer
{
    Task Consume(object msg, string rawMsg, string source, CancellationToken ct);
}

public abstract class MedusaConsumer<TMessage> : IMedusaConsumer
    where TMessage : class
{
    public Task Consume(object msg, string rawMsg, string source, CancellationToken ct)
        => Consume(new((TMessage)msg, rawMsg, source, ct));

    protected abstract Task Consume(ConsumeContext<TMessage> context);
}