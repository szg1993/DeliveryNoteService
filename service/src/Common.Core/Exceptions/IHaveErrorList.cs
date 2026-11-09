namespace Common.Core.Exceptions;

public interface IHaveErrorList
{
    IEnumerable<MedusaError> Errors { get; }
}