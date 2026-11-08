using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Medusa.Enap.Common.AspNetCore.Swagger;

internal sealed class SortPathsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        static string GetPrimaryTag(OpenApiPathItem pathItem)
        {
            var op = pathItem.Operations.Values.FirstOrDefault();
            var tag = op?.Tags?.FirstOrDefault()?.Name;

            return tag ?? string.Empty;
        }

        var ordered = swaggerDoc.Paths
            .OrderBy(p => GetPrimaryTag(p.Value), StringComparer.Ordinal)
            .ThenBy(p => p.Key, StringComparer.Ordinal)
            .ToList();

        var newPaths = new OpenApiPaths();
        foreach (var (key, value) in ordered)
            newPaths.Add(key, value);

        swaggerDoc.Paths = newPaths;
    }
}