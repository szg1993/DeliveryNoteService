using Medusa.Enap.Common.Core.Model;

namespace Medusa.Enap.DemoService.Domain.Models;

public class DemoModel : IModel
{
    public int Id { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? Name { get; set; }

    /// <summary>
    /// Incremented by a clustered job
    /// </summary>
    public int ClusteredJobCounter { get; set; }

    /// <summary>
    /// Incremented by a local job
    /// </summary>
    public int LocalJobCounter { get; set; }
    public Guid UniqueId { get; set; } = Guid.NewGuid();
}
