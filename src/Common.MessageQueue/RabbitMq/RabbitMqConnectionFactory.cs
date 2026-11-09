using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Common.MessageQueue.RabbitMq;

public class RabbitMqConnectionFactory(IOptions<MessageBrokerSettings> settings)
{
    public virtual async Task<IConnection> CreateConnectionAsync()
    {
        var s = settings.Value;
        var factory = new ConnectionFactory
        {
            HostName = s.Host,
            Port = s.Port,
            UserName = s.User,
            Password = s.Password,
            RequestedHeartbeat = TimeSpan.FromSeconds(s.RequestedHeartbeatSec),
            NetworkRecoveryInterval = TimeSpan.FromSeconds(s.NetworkRecoveryIntervalSec),
            AutomaticRecoveryEnabled = s.AutomaticRecoveryEnabled,
            TopologyRecoveryEnabled = s.TopologyRecoveryEnabled,
            VirtualHost = s.VirtualHost ?? ConnectionFactory.DefaultVHost
        };

        return await factory.CreateConnectionAsync();
    }
}