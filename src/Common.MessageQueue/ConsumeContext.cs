
namespace Common.MessageQueue;

public record ConsumeContext<TMessage>(
    TMessage Message,
    string RawMessage,
    string Source,
    CancellationToken CancellationToken
)
    where TMessage : class;