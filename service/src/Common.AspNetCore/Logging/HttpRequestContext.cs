using Common.Core.DependencyInjection;
using Common.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Common.AspNetCore.Logging;

[AutoRegister(serviceKey: typeof(IRequestContext))]
public class HttpRequestContext(IHttpContextAccessor httpContextAccessor) : IRequestContext
{
    public LogParams ToLogParams()
    {
        // This solution may be later moved to common and extended with a rabbitMQ/kafka implementation as well.

        if (httpContextAccessor?.HttpContext is null)
            return new LogParams();

        return LogParams.Create("apiRequest", httpContextAccessor.HttpContext.Request.GetDisplayUrl());
    }
}