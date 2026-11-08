namespace Medusa.Enap.Common.Core.Model;

public static class ModelEnumerableExtensions
{
    public static IEnumerable<int> SelectIds<TModel>(this IEnumerable<TModel> source)
        where TModel : IHaveId
        => source.Select(m => m.Id);

    public static bool Contains<TModel>(this IEnumerable<TModel> source, int id)
        where TModel : IHaveId
        => Find(source, id) is not null;

    public static TModel? Find<TModel>(this IEnumerable<TModel> source, int id)
        where TModel : IHaveId
        => source.FirstOrDefault(m => m.Id == id);

    public static IEnumerable<TModel> Find<TModel>(this IEnumerable<TModel> source, IEnumerable<int> ids)
        where TModel : IHaveId
        => source.Where(m => ids.Contains(m.Id));

    public static IEnumerable<string> SelectNos<TModel>(this IEnumerable<TModel> source)
        where TModel : IHaveNo
        => source.Select(m => m.No);

    public static TModel? FindByNo<TModel>(this IEnumerable<TModel> source, string no)
        where TModel : IHaveNo
        => source.FirstOrDefault(m => m.No == no);

    public static IQueryable<TModel> Except<TModel>(this IQueryable<TModel> source, IEnumerable<int> ids)
        where TModel : IHaveId
        => source.Where(m => !ids.Contains(m.Id));
}