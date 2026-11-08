using FluentAssertions;

namespace Medusa.Enap.Common.Testing;

public static class OpenApiSpecTester
{
    /// <summary>
    /// Intended to be wrapped in an ApiTest and simply called.
    /// Tests whether a newly downloaded openapi spec matches the one committed to the repo.
    /// Needs the openapi.json to be copied to the test bin folder.
    /// </summary>
    /// <param name="testApi"></param>
    /// <returns></returns>
    public static async Task AssertOpenApiSpecIsUpToDate(Api testApi)
    {
        var swaggerJsonResponse = await testApi.Client.GetAsync("/swagger/v1/swagger.json");
        var swaggerJson = await swaggerJsonResponse.Content.ReadAsStringAsync();
        swaggerJson = NormalizeLineEndings(swaggerJson);

        var committedOpenApiJson = await File.ReadAllTextAsync("openapi.json");
        committedOpenApiJson = NormalizeLineEndings(committedOpenApiJson);

        swaggerJson.Should().BeEquivalentTo(committedOpenApiJson, "always the most up-to-date OpenApi spec file should be committed to the repository");
        // if this test fails, run the generate_openapi_spec.cmd script and commit the updated openapi.json file
    }

    // The CLI tool generates with LF lineendings and git usually checks out with CRLF
    // Also, the openapi spec could contain multiline descriptions, so also normalize escaped line endings
    public static string NormalizeLineEndings(string s) =>
        s.Replace("\r\n", "\n")
         .Replace("\r", "\n")
         .Replace("\\r\\n", "\\n")
         .Replace("\\r", "\\n");
}