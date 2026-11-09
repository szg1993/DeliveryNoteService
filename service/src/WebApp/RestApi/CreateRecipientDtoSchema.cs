using DeliveryNoteService.Core.UseCases.Recipients;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DeliveryNoteService.WebApp.RestApi;

public class CreateRecipientDtoSchema : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(CreateRecipientDto))
            return;

        foreach (var property in schema.Properties)
        {
            schema.Required.Add(property.Key);
            property.Value.Nullable = false;
            property.Value.MinLength = 1;
        }
    }
}