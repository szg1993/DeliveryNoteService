using Medusa.Enap.Common.Core.Model;

namespace Medusa.Enap.Common.Core.UseCases;

public record Permissions
{
    public IEnumerable<ResourcePermission> ResourcePermissions { get; set; } = [];
}

public record ResourcePermission
{
    public ResourceType ResourceType { get; set; }
    public required string ResourceId { get; set; } 
}