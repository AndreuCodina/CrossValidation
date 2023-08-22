using System.Net;
using System.Runtime.CompilerServices;

namespace CrossValidation.Exceptions;

public abstract class ResxBusinessException : BusinessException
{
    public ResxBusinessException(
        string? key = null,
        HttpStatusCode statusCode = HttpStatusCode.UnprocessableEntity,
        string? details = null, 
        int statusCodeInt = (int)HttpStatusCode.UnprocessableEntity,
        [CallerArgumentExpression(nameof(key))]
        string? code = null)
        : base(
            code: code?.Substring(code.LastIndexOf('.') + 1),
            message: key ?? "",
            statusCode: statusCode,
            details: details,
            statusCodeInt: statusCodeInt)
    {
    }
}