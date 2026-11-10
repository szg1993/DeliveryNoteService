using System.Reflection;

namespace Common.Core.Versioning;

public interface ILocalVersionService
{
    Version GetVersion();
}

public class LocalVersionService : ILocalVersionService
{
    public Version GetVersion() => Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0);
}