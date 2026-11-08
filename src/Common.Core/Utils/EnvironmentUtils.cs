namespace Medusa.Enap.Common.Core.Utils;

public static class EnvironmentUtils
{
    /// <summary>
    /// Returns true if running in Visual Studio.
    /// More specifically if environment variable ASPNETCORE_ENVIRONMENT is set to "Development"
    /// </summary>
    /// <returns></returns>
    public static bool IsDevelopment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
    }
}
