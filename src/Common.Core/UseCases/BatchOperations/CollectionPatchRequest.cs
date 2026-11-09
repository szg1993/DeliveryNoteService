namespace Common.Core.UseCases.BatchOperations;

public class CollectionPatchRequest<TResourceDto>
{
    public List<TResourceDto> Add { get; set; } = [];

    public List<TResourceDto> Modify { get; set; } = [];

    public List<TResourceDto> Delete { get; set; } = [];
}