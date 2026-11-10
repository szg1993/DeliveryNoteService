namespace Common.MessageQueue;

public class MessageBrokerSettings
{
    public string Host { get; set; } = null!;

    public int Port { get; set; }

    public string User { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int RequestedHeartbeatSec { get; set; }

    public int NetworkRecoveryIntervalSec { get; set; }

    public bool AutomaticRecoveryEnabled { get; set; }

    public bool TopologyRecoveryEnabled { get; set; }

    public int MessageRetryDelaySeconds { get; set; }

    public string? VirtualHost { get; set; }
}