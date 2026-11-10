using System.Text;
using System.Text.Json.Nodes;

namespace ApiTests.TestSuite;

public static class NoteRequests
{
    public static async Task<HttpResponseMessage> PostRawNumberAsync(
        this DeliveryNoteServiceApi api,
        int recipientId,
        string path,
        string jsonNumber
    )
    {
        var request = Valid(recipientId);
        Set(request, path, JsonNode.Parse(jsonNumber));
        using var content = new StringContent(request.ToJsonString(), Encoding.UTF8, "application/json");
        return await api.Client.PostAsync("/api/delivery-notes", content);
    }

    public static JsonNode? Get(JsonNode request, string path)
    {
        JsonNode? value = request;
        foreach (var part in path.Split('.'))
            value = value is JsonArray array ? array[int.Parse(part)] : value![part];
        return value;
    }

    public static void Set(JsonObject request, string path, JsonNode? value)
    {
        var parts = path.Split('.');
        JsonNode parent = request;
        foreach (var part in parts[..^1])
            parent = parent is JsonArray array ? array[int.Parse(part)]! : parent[part]!;
        if (parent is JsonArray items)
            items[int.Parse(parts[^1])] = value;
        else
            parent[parts[^1]] = value;
    }

    public static void Remove(JsonObject request, string path)
    {
        var parts = path.Split('.');
        JsonNode parent = request;
        foreach (var part in parts[..^1])
            parent = parent is JsonArray array ? array[int.Parse(part)]! : parent[part]!;
        parent.AsObject().Remove(parts[^1]);
    }

    public static JsonObject Valid(int recipientId)
    {
        var request = JsonNode.Parse(
            """
            {
              "takeoverPlace": " Warehouse ", "category": " Goods ",
              "contactInfo": { "assignTo": " Driver ", "assignToPhone": " 123 ", "contact": " Receiver ", "contactPhone": " 456 " },
              "packageDetail": { "tareWgt": 0, "tareWgtUm": " kg ", "pkgQty": 1.25, "pkgScale": " pallet ", "pkgSizeX": 0, "pkgSizeY": 2.5, "pkgSizeZ": 3, "pkgSizeUm": " m " },
              "deadline": { "takeoverDate": "2020-01-01", "shipDate": "2020-01-01", "estimatedArrivalDate": "2020-01-02" },
              "noteLines": [ { "partCode": " A ", "partDesc": " Part  A ", "partQty": 2.25, "partQtyUm": " pcs ", "partWgt": 0, "partWgtUm": " kg " } ]
            }
            """
        )!.AsObject();
        request["recipientId"] = recipientId;
        return request;
    }
}