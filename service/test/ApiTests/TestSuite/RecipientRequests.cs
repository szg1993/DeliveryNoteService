using Medusa.Enap.DemoService.Application.Recipients;

namespace ApiTests.TestSuite;

public static class RecipientRequests
{
    public static CreateRecipientRequest WithField(string field, string? value)
        => field switch
        {
            "Code" => Valid() with { Code = value! },
            "Name" => Valid() with { Name = value! },
            "Country" => Valid() with { Country = value! },
            "Zip" => Valid() with { Zip = value! },
            "City" => Valid() with { City = value! },
            "Address" => Valid() with { Address = value! },
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, "Unknown recipient field.")
        };

    public static CreateRecipientRequest Valid(string code = "ACME", string name = "Acme Ltd")
        => new()
        {
            Code = code,
            Name = name,
            Country = "HU",
            Zip = "0101",
            City = "Budapest",
            Address = "Main street 1"
        };
}