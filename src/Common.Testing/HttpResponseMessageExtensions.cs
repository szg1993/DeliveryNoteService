using Common.AspNetCore.ErrorHandling;
using Common.Core;
using Common.Core.Exceptions;
using Common.Core.Utils;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Sdk;

namespace Common.Testing;

public static class HttpResponseMessageExtensions
{
    public static Task<T> ReadAsync<T>(this Task<HttpResponseMessage> responseTask, JsonSerializerOptions? jsonOptions = null)
        => responseTask.Then(response => response.ReadAsync<T>(jsonOptions));

    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response, JsonSerializerOptions? jsonOptions = null)
    {
        jsonOptions ??= JsonConfiguration.GetOptions();
        await response.AssertSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, jsonOptions) ?? throw new Exception("HTTP Response deserialization failed");
    }

    public static Task<HttpResponseMessage> AssertStatusCode(this Task<HttpResponseMessage> responseTask, HttpStatusCode expected)
        => responseTask.Then(response => response.AssertStatusCode(expected));

    public static async Task<HttpResponseMessage> AssertStatusCode(this HttpResponseMessage response, HttpStatusCode expected)
    {
        if (response.StatusCode == expected)
            return response;

        throw new XunitException(
            $"Status code '{response.StatusCode.ToStringWithCode()}' is not what was expected '{expected.ToStringWithCode()}'."
            + Environment.NewLine
            + await response.Content.ReadAsStringAsync());
    }

    public static Task<HttpResponseMessage> AssertNoContent(this Task<HttpResponseMessage> responseTask)
        => responseTask.Then(r => r.AssertNoContent());

    public static async Task<HttpResponseMessage> AssertNoContent(this HttpResponseMessage response)
    {
        await response.AssertStatusCode(HttpStatusCode.NoContent);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().BeNullOrEmpty();
        return response;
    }

    public static Task<HttpResponseMessage> AssertSuccessStatusCode(this Task<HttpResponseMessage> responseTask)
        => responseTask.Then(response => response.AssertSuccessStatusCode());

    public static async Task<HttpResponseMessage> AssertSuccessStatusCode(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return response;

        throw new XunitException(
            $"Status code '{response.StatusCode.ToStringWithCode()}' does not indicate success."
            + Environment.NewLine
            + await response.Content.ReadAsStringAsync());
    }

    public static Task<MedusaErrorResponse> AssertBadRequestAsync(this Task<HttpResponseMessage> response, string errorCode)
        => response.Then(r => r.AssertBadRequestAsync(errorCode));

    public static Task<MedusaErrorResponse> AssertBadRequestAsync(this HttpResponseMessage response, string errorCode)
        => response
            .AssertErrorAsync(HttpStatusCode.BadRequest)
            .AndErrorCodeAsync(errorCode);

    public static Task<MedusaErrorResponse> AssertNotFoundAsync(this Task<HttpResponseMessage> response, string errorCode)
        => response.Then(r => r.AssertNotFoundAsync(errorCode));

    public static Task<MedusaErrorResponse> AssertNotFoundAsync(this HttpResponseMessage response, string errorCode)
        => response
            .AssertErrorAsync(HttpStatusCode.NotFound)
            .AndErrorCodeAsync(errorCode);

    public static Task<MedusaErrorResponse> AssertUnauthorizedAsync(this Task<HttpResponseMessage> response, string errorCode = CommonErrorCodes.AuthenticationRequired)
        => response.Then(r => r.AssertUnauthorizedAsync(errorCode));

    public static Task<MedusaErrorResponse> AssertUnauthorizedAsync(this HttpResponseMessage response, string errorCode = CommonErrorCodes.AuthenticationRequired)
        => response
            .AssertErrorAsync(HttpStatusCode.Unauthorized)
            .AndErrorCodeAsync(errorCode);

    public static Task<MedusaErrorResponse> AssertForbiddenAsync(this Task<HttpResponseMessage> response, string errorCode = CommonErrorCodes.InsufficientPrivileges)
        => response.Then(r => r.AssertForbiddenAsync(errorCode));

    public static Task<MedusaErrorResponse> AssertForbiddenAsync(this HttpResponseMessage response, string errorCode = CommonErrorCodes.InsufficientPrivileges)
        => response
            .AssertErrorAsync(HttpStatusCode.Forbidden)
            .AndErrorCodeAsync(errorCode);

    public static async Task<MedusaErrorResponse> AndErrorCodeAsync(this Task<MedusaErrorResponse> errorTask, string errorCode)
    {
        var error = await errorTask;
        error.ErrorCode.Should().Be(errorCode);
        return error;
    }

    public static async Task<MedusaErrorResponse> AndErrorDataAsync(this Task<MedusaErrorResponse> errorTask, string key, string[] expectedValue)
    {
        var error = await errorTask;
        error.Data.TryGetValue(key, out var errorData).Should().BeTrue();
        errorData.Should().NotBeNull();
        var errorValues = ((JsonElement)errorData!).EnumerateArray().Select(e => e.GetString()).ToArray();
        errorValues.Should().BeEquivalentTo(expectedValue);
        return error;
    }

    public static async Task<MedusaErrorResponse> AndErrorDataAsync(this Task<MedusaErrorResponse> errorTask, string key, string expectedValue)
    {
        var error = await errorTask;
        error.Data.TryGetValue(key, out var errorData).Should().BeTrue();
        errorData.Should().NotBeNull();
        var errorValues = ((JsonElement)errorData!).GetString();
        errorValues.Should().BeEquivalentTo(expectedValue);
        return error;
    }

    public static async Task<MedusaErrorResponse> AndErrorDataAsync(this Task<MedusaErrorResponse> errorTask, string key, int expectedValue)
    {
        var error = await errorTask;
        error.Data.TryGetValue(key, out var errorData).Should().BeTrue();
        errorData.Should().NotBeNull();
        var errorValues = ((JsonElement)errorData!).GetInt32();
        errorValues.Should().Be(expectedValue);
        return error;
    }

    public static Task<MedusaErrorResponse> AssertErrorAsync(this Task<HttpResponseMessage> response, HttpStatusCode? expectedStatusCode = null)
        => response.Then(r => r.AssertErrorAsync(expectedStatusCode));

    public static async Task<MedusaErrorResponse> AssertErrorAsync(this HttpResponseMessage response, HttpStatusCode? expectedStatusCode = null)
    {
        if (expectedStatusCode is not null)
            await response.AssertStatusCode(expectedStatusCode.Value);

        return await response.Content.ReadFromJsonAsync<MedusaErrorResponse>()
            ?? throw new XunitException("Expected to find an Error in the response body but found null instead");
    }

    public static void AssertError(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            throw new XunitException("Expected to find an error status code but the response was successful");
    }

    private static string ToStringWithCode(this HttpStatusCode statusCode) => $"{statusCode} ({(int)statusCode})";
}