using Medusa.Enap.Common.Core.Versioning;

namespace Medusa.Enap.Common.Testing.Versioning;

public class TestVersionService : ILocalVersionService
{
    private Version _version = new(1, 0, 0, 0);
    public Version GetVersion() => _version;
    public void SetVersion(Version version) => _version = version;
}
