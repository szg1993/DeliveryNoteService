namespace Medusa.Enap.Common.Core.Exceptions;

public interface IHaveErrorList
{
    IEnumerable<MedusaError> Errors { get; }
}
