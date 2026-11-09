using System.Reflection;

namespace Common.Core.Utils;

public static class AssemblyUtils
{
    public static IEnumerable<Assembly> GetMedusaAssemblies() =>
        AppDomain.CurrentDomain.GetAssemblies()
            //.Where(a =>
            //    a.FullName!.StartsWith("DeliveryNoteService", StringComparison.InvariantCultureIgnoreCase)
            //    || a.FullName!.StartsWith("Common", StringComparison.InvariantCultureIgnoreCase)
            //)
            ;
}