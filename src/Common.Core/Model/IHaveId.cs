namespace Medusa.Enap.Common.Core.Model;

/// <summary>
/// Entities with unique integer ID
/// </summary>
public interface IHaveId : IHaveId<int>;

/// <summary>
/// Entities with unique ID of type TId
/// </summary>
public interface IHaveId<out TId>
{
    public TId Id { get; }
}
