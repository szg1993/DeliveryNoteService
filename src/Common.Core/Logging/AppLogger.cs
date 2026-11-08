using Medusa.Enap.Common.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Medusa.Enap.Common.Core.Logging;

/// <summary>
/// To be used in an application event like startup, shutdown, where there is no use case, no logic class.
/// CorrelationId will be initialized to a new GUID for the lifetime of this logger
/// </summary>
[AutoRegister(ServiceLifetime.Transient)]
public class AppLogger : MedusaLogger<AppLogger>
{
    public AppLogger(ILogger<AppLogger> logger) : base(logger)
    {
        PushCorrelationIdProperty(correlationId: Guid.NewGuid().ToString());
    }
}
