using System.Net;
using CrossValidation.Errors;

namespace CrossValidation;

public interface IValidationCustomizations
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public ICrossError? Error { get; set; }
    public string? FieldDisplayName { get; set; }
    public HttpStatusCode? HttpStatusCode { get; set; }
    public bool ExecuteValidator { get; set; }
}