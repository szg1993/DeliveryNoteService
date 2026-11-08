using System.Reflection;

namespace Medusa.Enap.Common.Core.Utils;

public static class AssemblyUtils
{
    public static IEnumerable<Assembly> GetMedusaAssemblies() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName!.StartsWith("Medusa", StringComparison.InvariantCultureIgnoreCase));
}
