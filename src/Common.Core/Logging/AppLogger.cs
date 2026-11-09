using Common.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.Core.Logging;

/// <summary>
/// To be used in an application event like startup, shutdown, where there is no use case, no logic class.
/// CorrelationId will be initialized to a new GUID for the lifetime of this logger
/// </summary>
[AutoRegister(ServiceLifetime.Transient)]
public class AppLogger : DeliveryNoteServiceLogger<AppLogger>
{
    public AppLogger(ILogger<AppLogger> logger) : base(logger)
    {
        PushCorrelationIdProperty(correlationId: Guid.NewGuid().ToString());
    }
}