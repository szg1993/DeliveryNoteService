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

    [Fact]
    public async Task GeneratedDocument_DescribesDetailAndCreationLocation()
    {
        var document = (await Api.Client.GetFromJsonAsync<JsonObject>("/swagger/v1/swagger.json"))!;
        var schemas = document["components"]!["schemas"]!;
        var paths = document["paths"]!;
        var operation = paths["/api/delivery-notes/{id}"]!["get"]!;

        operation["responses"]!.AsObject().Select(pair => pair.Key).Should().BeEquivalentTo("200", "400", "404");
        operation["security"]?.AsArray().Should().BeEmpty();
        operation["description"]!.GetValue<string>().Should().Contain("Authentication is not required");
        var id = operation["parameters"]!.AsArray().Single()!;
        id["name"]!.GetValue<string>().Should().Be("id");
        id["in"]!.GetValue<string>().Should().Be("path");
        id["required"]!.GetValue<bool>().Should().BeTrue();
        id["schema"]!["type"]!.GetValue<string>().Should().Be("integer");
        var postResponse = paths["/api/delivery-notes"]!["post"]!["responses"]!["201"]!;
        postResponse["headers"]!["Location"]!["schema"]!["format"]!.GetValue<string>().Should().Be("uri-reference");
        operation["responses"]!["200"]!["content"]!["application/json"]!["schema"]!["$ref"]!.GetValue<string>()
            .Should().Be(postResponse["content"]!["application/json"]!["schema"]!["$ref"]!.GetValue<string>());
        schemas["NoteDetailsDto"]!["properties"]!.AsObject().Select(pair => pair.Key).Should().Contain(
            "id",
            "noteNbr",
            "createdDate",
            "noteStatusId",
            "recipient",
            "contactInfo",
            "packageDetail",
            "deadline",
            "noteLines"
        );
        foreach (var name in new[] { "takeoverDate", "shipDate", "estimatedArrivalDate" })
            schemas["DeadlineDetailsDto"]!["properties"]![name]!["format"]!.GetValue<string>().Should().Be("date");
    }
}