using FluentAssertions;
using Medusa.Enap.Common.AspNetCore.Swagger.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Net;
using System.Net.Mime;

namespace UnitTests.AspnetCore;

public class OperationFilterTests
{
    private class TestApiEndpointParams
    {
        [Description("This is a string filter.")]
        public string? Name { get; set; }

        [Description("This is an integer filter.")]
        public int? Count { get; set; }

        public bool? Active { get; set; } = null!;
    }

    [Fact]
    public async Task Headers()
    {
        var app = CreateSwaggerTestWebApp(c => c
            .OperationFilter<HeaderParameterFilter>()
        );
        app
            .MapGet("/test",
                (
                    [FromHeader(Name = "Required-Test-Header-1")] string requiredHeader,
                    [FromHeader(Name = "Optional-Test-Header-1")] string? optionalHeader
                )
                => Results.Ok()
            )
            .RequireHeader("Required-Test-Header-2")
            .WithOptionalHeader("Optional-Test-Header-2")
            ;

        var swagger = await GetOpenApiDocument(app);

        var op = swagger.Paths["/test"].Operations[OperationType.Get];
        Assert("Required-Test-Header-1", true);
        Assert("Optional-Test-Header-1", false);
        Assert("Required-Test-Header-2", true);
        Assert("Optional-Test-Header-2", false);
        void Assert(string name, bool required)
        {
            op.Parameters.Should().ContainSingle(p => p.Name == name)
                .Which.Required.Should().Be(required);
        }
    }

    [Fact]
    public async Task ParameterDescriptions()
    {
        var app = CreateSwaggerTestWebApp(c => c
            .OperationFilter<ParameterDescriptionFilter>()
        );
        app.MapGet("/test/{Id}", ([AsParameters] TestApiEndpointParams p, [FromRoute] int id) => Results.Ok());

        var swagger = await GetOpenApiDocument(app);

        Assert.NotEmpty(swagger.Paths);
        var op = swagger.Paths["/test/{Id}"].Operations[OperationType.Get];
        op.Parameters.Should().HaveCount(4);
        op.Parameters.Should().Contain(p => p.Name == "name" && p.Description == "This is a string filter.");
        op.Parameters.Should().Contain(p => p.Name == "count" && p.Description == "This is an integer filter.");
        op.Parameters.Should().Contain(p => p.Name == "active" && p.Description == null);
        // do not convert header keys to camelCase
        op.Parameters.Should().Contain(p => p.Name == "Id" && p.Description == null);
    }

    [Fact]
    public async Task Responses()
    {
        var app = CreateSwaggerTestWebApp(c => c
            .OperationFilter<ResponseDescriptionFilter>()
        );
        app.MapGet("/test", () => Results.Ok(42))
            .ProducesUnauthorized()
            .ProducesForbidden()
            .ProducesOk<int>("The answer to the ultimate question of life, the universe, and everything");

        var swagger = await GetOpenApiDocument(app);

        var op = swagger.Paths.Single().Value.Operations[OperationType.Get];
        op.AssertResponse(HttpStatusCode.Unauthorized, "Authentication required");
        op.AssertResponse(HttpStatusCode.Forbidden, "No permission.");
        op.AssertResponse(HttpStatusCode.OK, "The answer to the ultimate question of life, the universe, and everything",
            expectedResponseType: "integer");
    }

    private static async Task<OpenApiDocument> GetOpenApiDocument(WebApplication app)
    {
        await app.StartAsync();
        using var scope = app.Services.CreateScope();
        var swaggerProvider = scope.ServiceProvider.GetRequiredService<ISwaggerProvider>();
        var swagger = swaggerProvider.GetSwagger("v1");
        await app.StopAsync();
        await app.DisposeAsync();
        return swagger;
    }

    private static WebApplication CreateSwaggerTestWebApp(Action<SwaggerGenOptions>? setupAction = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c => setupAction?.Invoke(c));
        return builder.Build();
    }
}

public static class OpenApiOperationExtensions
{
    public static void AssertResponse(this OpenApiOperation op, HttpStatusCode expectedStatusCode, string? expectedDescription, string? expectedResponseType = null)
    {
        var openApiResponse = op.Responses[((int)expectedStatusCode).ToString()];
        openApiResponse.Description.Should().Be(expectedDescription);
        if (expectedResponseType != null)
            openApiResponse.Content[MediaTypeNames.Application.Json].Schema.Type.Should().Be(expectedResponseType);
    }
}
