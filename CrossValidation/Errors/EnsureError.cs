using System.Net;
using CrossValidation.Exceptions;

namespace CrossValidation.Errors;

public record EnsureError
{
    public string? Details { get; set; }
    public HttpStatusCode? HttpStatusCode { get; set; }
    
    public EnsureException ToException()
    {
        return new EnsureException(this);
    }
}