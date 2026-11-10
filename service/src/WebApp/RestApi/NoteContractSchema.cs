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
            [typeof(CreateNoteLineDto)] = new Dictionary<string, int>
            {
                ["partCode"] = NoteLimits.PartCode,
                ["partDesc"] = NoteLimits.PartDesc,
                ["partCmt"] = NoteLimits.PartCmt,
                ["partQtyUm"] = NoteLimits.PartQtyUm,
                ["partWgtUm"] = NoteLimits.PartWgtUm,
            },
            [typeof(CreateNoteDto)] = new Dictionary<string, int>
            {
                ["ekaerNbr"] = NoteLimits.EkaerNbr,
                ["takeoverPlace"] = NoteLimits.TakeoverPlace,
                ["category"] = NoteLimits.Category,
                ["comments"] = NoteLimits.Comments,
            },
        };

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
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
                property.Description = "Surrounding whitespace is removed before length validation and storage.";
            }
            if (property.Type is "number" or "integer")
            {
                property.Minimum = name == "recipientId" ? 1 : 0;
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
}