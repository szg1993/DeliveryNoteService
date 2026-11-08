namespace Medusa.Enap.DemoService.Application;

public record DemoMessage
{
    public string? Name { get; set; }

    public bool MakeApiCall { get; set; }
}
