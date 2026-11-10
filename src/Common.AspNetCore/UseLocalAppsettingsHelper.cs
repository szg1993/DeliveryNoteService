using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Common.AspNetCore;

public static class UseLocalAppsettingsHelper
{
    public static WebApplicationBuilder UseLocalAppsettings(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
            builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

        return builder;
    }
}