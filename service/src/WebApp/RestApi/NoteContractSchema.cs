using DeliveryNoteService.Core.Models;
using DeliveryNoteService.Core.UseCases.Notes;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DeliveryNoteService.WebApp.RestApi;

public sealed class NoteContractSchema : ISchemaFilter
{
    private static readonly IReadOnlyDictionary<Type, IReadOnlyDictionary<string, int>> _textLimits =
        new Dictionary<Type, IReadOnlyDictionary<string, int>>
        {
            [typeof(CreateContactInfoDto)] = new Dictionary<string, int>
            {
                ["assignTo"] = NoteLimits.AssignTo,
                ["assignToPhone"] = NoteLimits.AssignToPhone,
                ["contact"] = NoteLimits.Contact,
                ["contactPhone"] = NoteLimits.ContactPhone,
            },
            [typeof(CreatePackageDetailDto)] = new Dictionary<string, int>
            {
                ["tareWgtUm"] = NoteLimits.TareWgtUm,
                ["pkgScale"] = NoteLimits.PkgScale,
                ["pkgSizeUm"] = NoteLimits.PkgSizeUm,
            },
            [typeof(CreateDeadlineDto)] = new Dictionary<string, int>(),
            [typeof(CreateDeliveryNoteLineDto)] = new Dictionary<string, int>
            {
                ["partCode"] = NoteLimits.PartCode,
                ["partDesc"] = NoteLimits.PartDesc,
                ["partCmt"] = NoteLimits.PartCmt,
                ["partQtyUm"] = NoteLimits.PartQtyUm,
                ["partWgtUm"] = NoteLimits.PartWgtUm,
            },
            [typeof(CreateDeliveryNoteDto)] = new Dictionary<string, int>
            {
                ["ekaerNbr"] = NoteLimits.EkaerNbr,
                ["takeoverPlace"] = NoteLimits.TakeoverPlace,
                ["category"] = NoteLimits.Category,
                ["comments"] = NoteLimits.Comments,
            },
        };

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        DescribeDecimals(schema, context.Type);
        if (context.Type == typeof(NoteStatus))
        {
            schema.Type = "string";
            schema.Format = null;
            schema.Enum = Enum.GetNames<NoteStatus>().Select(name => (IOpenApiAny)new OpenApiString(name)).ToList();
        }
        if (!_textLimits.TryGetValue(context.Type, out var limits))
            return;

        schema.AdditionalPropertiesAllowed = false;
        foreach (var (name, property) in schema.Properties)
        {
            var optional = name is "ekaerNbr" or "comments" or "partCmt";
            if (name.StartsWith("is", StringComparison.Ordinal))
            {
                property.Default = new OpenApiBoolean(false);
                continue;
            }
            property.Nullable = optional;
            if (!optional)
                schema.Required.Add(name);
            if (limits.TryGetValue(name, out var limit))
            {
                property.MaxLength = limit;
                property.MinLength = optional ? null : 1;
                property.Description = "Text is validated and stored exactly as supplied; surrounding whitespace counts toward the length limit.";
            }
            if (property.Type is "number" or "integer")
            {
                property.Minimum = name == "recipientId" ? null : 0;
                property.ExclusiveMinimum = name is "pkgQty" or "partQty";
            }
            if (context.Type == typeof(CreateDeadlineDto))
            {
                property.Type = "string";
                property.Format = "date";
                property.Pattern = @"^\d{4}-\d{2}-\d{2}$";
                property.Description = "Required calendar date, exactly yyyy-MM-dd. TakeoverDate <= ShipDate <= EstimatedArrivalDate; equal and past dates are allowed.";
            }
            if (name == "noteLines")
            {
                property.MinItems = 1;
                property.Items.Nullable = false;
                property.Description = "Ordered nonempty lines; repeated part codes remain separate. No maximum count.";
            }
        }
    }

    private static void DescribeDecimals(OpenApiSchema schema, Type type)
    {
        if (type != typeof(CreatePackageDetailDto) &&
            type != typeof(CreateDeliveryNoteLineDto) &&
            type != typeof(PackageDetailDetailsDto) &&
            type != typeof(NoteLineDetailsDto)
        )
        {
            return;
        }

        foreach (var property in schema.Properties.Values.Where(property => property.Type == "number"))
        {
            property.Format = "decimal";
            property.Description = "An exact base-10 decimal JSON number. Values must be representable as a .NET decimal without rounding; use a decimal-capable client. No business precision or scale cap applies.";
        }
    }
}