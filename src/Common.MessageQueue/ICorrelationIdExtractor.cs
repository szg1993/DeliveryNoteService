namespace Medusa.Enap.Common.MessageQueue;

public interface ICorrelationIdExtractor
{
    string GetCorrelationId(object message);
}
