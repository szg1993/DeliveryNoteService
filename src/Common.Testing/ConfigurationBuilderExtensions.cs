using Microsoft.Extensions.Configuration;

namespace Common.Testing;

public static class ConfigurationBuilderExtensions
{
    public static void AddConnectionString(this IConfigurationBuilder builder, string connectionString)
        => Add(builder, "Persistence:ConnectionStrings:AppDb", connectionString);

    public static void Add(this IConfigurationBuilder builder, string key, string value)
    {
        var setting = KeyValuePair.Create(key, (string?)value);
        builder.AddInMemoryCollection([setting]);
    }

    public static void DisableScheduledJob(this IConfigurationBuilder builder, string jobKey)
    {
        const string never = "0 0 0 24 12 ? 2100";
        builder.Add("Scheduling:" + jobKey, never);
    }

    public static void DisableStartupJobs(this IConfigurationBuilder builder, bool disable)
        => builder.Add("Scheduling:DisableStartupJobs", disable.ToString());
}