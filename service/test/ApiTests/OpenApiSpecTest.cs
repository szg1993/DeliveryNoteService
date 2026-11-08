namespace ApiTests;

public class OpenApiSpecTest : DemoServiceApiTestBase
{
    [Fact]
    public Task OpenApiFileIsUpToDate() => OpenApiSpecTester.AssertOpenApiSpecIsUpToDate(Api);
}
