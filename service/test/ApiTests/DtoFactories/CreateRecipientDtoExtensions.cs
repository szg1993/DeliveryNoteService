using DeliveryNoteService.Core.UseCases.Recipients;

namespace ApiTests.DtoFactories;

public static class CreateRecipientDtoExtensions
{
    public static CreateRecipientDto WithCode(this CreateRecipientDto self, string code)
    {
        return self with { Code = code };
    }

    public static CreateRecipientDto WithName(this CreateRecipientDto self, string name)
    {
        return self with { Name = name };
    }

    public static CreateRecipientDto WithCountry(this CreateRecipientDto self, string country)
    {
        return self with { Country = country };
    }

    public static CreateRecipientDto WithZip(this CreateRecipientDto self, string zip)
    {
        return self with { Zip = zip };
    }

    public static CreateRecipientDto WithCity(this CreateRecipientDto self, string city)
    {
        return self with { City = city };
    }

    public static CreateRecipientDto WithAddress(this CreateRecipientDto self, string address)
    {
        return self with { Address = address };
    }
}