using Medusa.Enap.Common.Core.Model;

namespace Medusa.Enap.DemoService.Domain.Models;

public class Recipient : IModel
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Country { get; set; } = "";
    public string Zip { get; set; } = "";
    public string City { get; set; } = "";
    public string Address { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}