using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace ApiTests;

public class NoteDocumentationTests : DeliveryNoteServiceApiTestBase
{
    protected override void ConfigureTestHostConfiguration(IConfigurationBuilder configuration)
    {
        configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["Swagger:Enabled"] = "true" });
    }

    [Fact]
    public async Task GeneratedDocument_DescribesStrictCreationContract()
    {
        var document = (await Api.Client.GetFromJsonAsync<JsonObject>("/swagger/v1/swagger.json"))!;
        var schemas = document["components"]!["schemas"]!;
        var operation = document["paths"]!["/api/delivery-notes"]!["post"]!;

        operation["responses"]!.AsObject().Select(pair => pair.Key).Should().BeEquivalentTo("201", "400", "404");
        operation["security"]?.AsArray().Should().BeEmpty();
        foreach (var name in new[] { "CreateNoteDto", "CreateContactInfoDto", "CreatePackageDetailDto", "CreateDeadlineDto", "CreateNoteLineDto" })
        {
            schemas[name]!["additionalProperties"]!.GetValue<bool>().Should().BeFalse();
            schemas[name]!["required"]!.AsArray().Should().NotBeEmpty();
            schemas[name]!["properties"]!["id"].Should().BeNull();
        }
        var note = schemas["CreateNoteDto"]!;
        note["properties"]!["takeoverPlace"]!["maxLength"]!.GetValue<int>().Should().Be(30);
        note["properties"]!["noteLines"]!["minItems"]!.GetValue<int>().Should().Be(1);
        schemas["CreatePackageDetailDto"]!["properties"]!["pkgQty"]!["exclusiveMinimum"]!.GetValue<bool>().Should().BeTrue();
        schemas["CreateDeadlineDto"]!["properties"]!["takeoverDate"]!["format"]!.GetValue<string>().Should().Be("date");
    }
}