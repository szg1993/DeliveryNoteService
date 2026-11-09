using Common.Core.Exceptions;
using System.Text.Json.Serialization;

namespace Common.AspNetCore.ErrorHandling;

public class MedusaErrorResponse : MedusaError
{
    [JsonConstructor]
    public MedusaErrorResponse(string errorCode) : base(errorCode) { }
    public MedusaErrorResponse(Exception ex) : base(ex) { }
    public string? CorrelationId { get; set; }
}