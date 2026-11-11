using Common.AspNetCore.ErrorHandling;
using Common.Core;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Common.Testing;

public static class TestHttpClientHelpers
{
    public static IServiceCollection AddTestHttpClient(this IServiceCollection services)
        => services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory());

    public static MockHttpMessageHandler HttpClient(this IScenario scenario)
        => scenario.Services.HttpClient();

    public static MockHttpMessageHandler HttpClient(this IScenario scenario, Action<MockHttpMessageHandler> setter)
        => scenario.Services.HttpClient(setter);

    public static MockHttpMessageHandler HttpClient(this IServiceProvider services, Action<MockHttpMessageHandler> setter)
    {
        var handler = services.HttpClient();
        setter(handler);
        return handler;
    }

    public static MockHttpMessageHandler HttpClient(this IServiceProvider services)
    {
        return services
            .GetRequiredService<IHttpClientFactory>()
            .As<TestHttpClientFactory>()
            .MessageHandler;
    }

    /// <summary>
    /// Shortcut to WithJsonContent with Medusa JsonOptions
    /// </summary>
    public static MockedRequest WithBody<T>(this MockedRequest source, Func<T, bool> predicate)
        where T : class
            => source.WithJsonContent<T>(predicate, JsonConfiguration.GetOptions());

    public static MockedRequest Then(this MockedRequest source, Action<HttpRequestMessage> callback)
    {
        return source.With(req =>
        {
            callback(req);
            return true;
        });
    }

    public static MockedRequest RespondEmptyStream(this MockedRequest source)
       => RespondStream(source, new MemoryStream(Encoding.UTF8.GetBytes("")));

    public static MockedRequest RespondStream(this MockedRequest source, Stream stream)
    {
        return source.Respond(_ => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            }
        ));
    }

    public static MockedRequest RespondJson(this MockedRequest source, Func<HttpRequestMessage, object> func)
    {
        return source.RespondJson((HttpRequestMessage m) => Task.FromResult(func(m)));
    }

    public static MockedRequest RespondJson(this MockedRequest source, Func<HttpRequestMessage, Task<object>> func)
    {
        return source.Respond(async req =>
        {
            var content = await func(req);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(content, options: JsonConfiguration.GetOptions())
            };
        });
    }

    public static MockedRequest RespondJson(this MockedRequest source, object content)
    {
        var json = JsonSerializer.Serialize(content, JsonConfiguration.GetOptions());
        return source.Respond("application/json", json);
    }

    public static MockedRequest RespondJsonError(this MockedRequest source,
        HttpStatusCode statusCode, string errorCode)
    {
        var error = new MedusaErrorResponse(errorCode);
        var json = JsonSerializer.Serialize(error, JsonConfiguration.GetOptions());
        return source.Respond(statusCode, MediaTypeNames.Application.Json, json);
    }

    public static MockedRequest WithAuthToken(this MockedRequest source, string token)
        => source.WithHeaders("Authorization", "Bearer " + token);
}

public class TestHttpClientFactory : IHttpClientFactory
{
    public MockHttpMessageHandler MessageHandler { get; } = new MockHttpMessageHandler(BackendDefinitionBehavior.Always);

    public HttpClient CreateClient(string name) => new(MessageHandler);
}