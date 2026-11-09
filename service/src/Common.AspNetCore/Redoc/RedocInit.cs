using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Common.AspNetCore.Redoc;

public static class RedocInit
{
    public static IApplicationBuilder UseMedusaRedoc(this IApplicationBuilder app, IConfigurationSection settings)
    {
        if (settings.Get<RedocSettings>()?.Enabled == true)
        {
            app.UseReDoc(options =>
            {
                options.SpecUrl("/swagger/v1/swagger.json");
                options.RoutePrefix = "docs";
                options.HideDownloadButton();
                options.PathInMiddlePanel();
                options.EnableUntrustedSpec();
                options.ScrollYOffset(10);
                options.HideHostname();
                options.ExpandResponses("all");
                options.RequiredPropsFirst();
                options.NoAutoAuth();
                options.HideLoading();
                options.NativeScrollbars();
                options.OnlyRequiredInSamples();
                options.SortPropsAlphabetically();
            });
        }

        return app;
    }
}