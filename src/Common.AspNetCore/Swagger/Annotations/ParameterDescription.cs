using Common.Core.Utils;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Reflection;

namespace Common.AspNetCore.Swagger.Annotations;

public class ParameterDescriptionFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            return;

        foreach (var parameter in operation.Parameters)
            Apply(parameter, context);
    }

    private static void Apply(OpenApiParameter parameter, OperationFilterContext context)
    {
        var apiParam = context.ApiDescription.ParameterDescriptions
            .FirstOrDefault(p => string.Equals(p.Name, parameter.Name, StringComparison.OrdinalIgnoreCase));

        var description = (apiParam?.ParameterDescriptor as IParameterInfoParameterDescriptor)
            ?.ParameterInfo
            .GetCustomAttribute<DescriptionAttribute>()
            ?.Description;
        parameter.Description = description ?? parameter.Description;
        if (parameter.In == ParameterLocation.Query)
            parameter.Name = parameter.Name.ToCamelCase();
    }
}