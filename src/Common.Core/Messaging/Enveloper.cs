using Common.Core.Utils;

namespace Common.Core.Messaging;

public static class Enveloper
{
    public static Envelope<T> Envelop<T>(this T self)
        => new(self);

    public static PagedEnvelope<T> Envelop<T>(this IEnumerable<T> self, PageDetails pageDetails)
        => new(self, pageDetails);

    public static Task<PagedEnvelope<T>> Envelop<T>(this Task<(PageDetails page, IEnumerable<T> result)> task)
        => task.Then(s => Envelop(s));

    public static PagedEnvelope<T> Envelop<T>(this (PageDetails page, IEnumerable<T> result) self)
        => new(self.result, self.page);
}