namespace Medusa.Enap.Common.Core.Versioning;

public class VersionInfo
{
    public string ComponentName { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public IEnumerable<VersionInfo>? ConnectedServices { get; set; }
}
