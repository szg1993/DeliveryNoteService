using DeliveryNoteService.Core.UseCases.Recipients.Data;

namespace ApiTests.TestSuite;

public static class RecipientRequests
{
    public static CreateRecipientDto Valid(string code = "ACME", string name = "Acme Ltd")
        => new()
        {
            Code = code,
            Name = name,
            Country = "HU",
            Zip = "0101",
            City = "Budapest",
            Address = "Main street 1"
        };

    public static UpdateRecipientDto ValidUpdate()
        => new()
        {
            Name = "Globex Kft",
            Country = "HU",
            Zip = "6720",
            City = "Szeged",
            Address = "Kossuth street 2"
        };
}