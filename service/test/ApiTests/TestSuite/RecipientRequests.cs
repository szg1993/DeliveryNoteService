using DeliveryNoteService.Core.UseCases.Recipients;

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
}