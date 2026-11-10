using Common.Core.Utils;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Common.AspNetCore.Swagger;

class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum && schema.AllOf.IsNullOrEmpty())
        {
            schema.Type = "string";
            schema.Format = null;
            schema.Enum = Enum.GetNames(context.Type)
                .Select(name => new OpenApiString(name))
                .Cast<IOpenApiAny>()
                .ToList();
        }
    }
}