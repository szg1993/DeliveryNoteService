using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Medusa.Enap.DemoService.Application.Recipients;

public record CreateRecipientRequest
{
    [Required]
    [Description("Manually entered unique code. Trimmed and uppercased; 1-10 ASCII letters, digits, hyphens, or underscores after normalization.")]
    public string Code { get; init; } = "";
    [Required]
    [Description("Recipient name, up to 200 characters after trimming. Duplicate names are allowed.")]
    public string Name { get; init; } = "";
    [Required]
    [Description("Recognized two-letter country code, trimmed and uppercased. HU is domestic; other countries are foreign.")]
    public string Country { get; init; } = "";
    [Required]
    [Description("ZIP or postal code as text, up to 20 characters after trimming. Leading zeros are preserved; no postal-format verification is performed.")]
    public string Zip { get; init; } = "";
    [Required]
    [Description("Destination city, up to 50 characters after trimming.")]
    public string City { get; init; } = "";
    [Required]
    [Description("Destination address, up to 200 characters after trimming. Address existence is not verified.")]
    public string Address { get; init; } = "";
}