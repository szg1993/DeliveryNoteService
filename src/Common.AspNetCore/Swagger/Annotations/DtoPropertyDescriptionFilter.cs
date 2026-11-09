using Common.Core.Utils;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Reflection;

namespace Common.AspNetCore.Swagger.Annotations;

public class DtoPropertyDescriptionFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema?.Properties == null || context.Type == null)
            return;

        var properties = context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var descriptionAttr = property.GetCustomAttribute<DescriptionAttribute>();

            if (descriptionAttr is null)
                continue;

            if (schema.Properties.TryGetValue(property.Name.ToCamelCase(), out var schemaProperty))
                schemaProperty.Description = descriptionAttr.Description;
        }
    }
}