using DeliveryNoteService.Core.UseCases.Recipients.Data;

namespace ApiTests.DtoFactories;

public static class DtoFactory
{
    public static CreateRecipientDto CreateRecipient()
    {
        return new CreateRecipientDto
        {
            Code = "DummyCode",
            Name = "Dummy Name",
            Country = "HU",
            Zip = "1234",
            City = "Dummy City",
            Address = "Dummy Address"
        };
    }

    public static UpdateRecipientDto UpdateRecipient()
    {
        return new UpdateRecipientDto
        {
            Name = "Dummy Name",
            Country = "HU",
            Zip = "1234",
            City = "Dummy City",
            Address = "Dummy Address"
        };
    }
}