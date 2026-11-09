using FluentAssertions;
using Common.Core.Messaging;
using Common.Core.UseCases.BatchOperations;

namespace Common.Testing.ApiWrappers;

public class BatchEndpoint<TResource>(string url, HttpClient client)
    : ApiEndpoint(url, client)
{
    public async Task<BatchResultItem<TResource>[]> PostAsync(BatchRequestDto<TResource> dto)
    {
        var response = await PostRawAsync(path: null, dto);
        var parsed = await response.ReadAsync<Envelope<BatchResultItem<TResource>[]>>(JsonOptions);
        return parsed.Content;
    }

    [Obsolete("Batch operation unification in progress. Use the other overload instead.")]
    public async Task<TResource[]> PostAsync(object dto)
    {
        var response = await PostRawAsync(path: null, dto);
        var parsed = await response.ReadAsync<Envelope<TResource[]>>(JsonOptions);
        return parsed.Content;
    }
}

public static class EndpointExtensions
{
    [Obsolete("Batch operation unification in progress. Use the other overload instead.")]
    public static BatchEndpoint<TResource> Batch<TResource>(this CollectionEndpoint<TResource> parent)
        => new(parent.Url + "/batch", parent.Client);

    // Discuss with architects: unify batch operation extensions to REST,
    //                          create technical stories to adjust existing endpoints
    public static BatchEndpoint<TResource> Batch<TResource>(this ApiEndpoint parent)
        => new(parent.Url + "/batch", parent.Client);
}

public static class BatchResultExtensions
{
    public static void AssertNoErrors<T>(this BatchResultItem<T> resultItem)
    {
        resultItem.ErrorCode.Should().BeNull();
    }

    public static BatchResultItem<T> AssertError<T>(this BatchResultItem<T> resultItem, string expectedErrorCode, Dictionary<string, string>? expectedErrorData = null)
    {
        resultItem.ErrorCode.Should().Be(expectedErrorCode);

        if (expectedErrorData is not null)
            resultItem.ErrorData.Should().Contain([.. expectedErrorData]);  // expectation has been specified
        else
            resultItem.ErrorData.Should().NotBeNull(); // no expectations but it must not be null

        return resultItem;
    }
}