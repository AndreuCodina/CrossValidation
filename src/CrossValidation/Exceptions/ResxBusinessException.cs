using System.Net;
using System.Runtime.CompilerServices;

namespace CrossValidation.Exceptions;

public class ResxBusinessException(
    string? key = null,
    HttpStatusCode statusCode = HttpStatusCode.UnprocessableEntity,
    string? details = null,
    [CallerArgumentExpression(nameof(key))] string? code = null)
    : BusinessException(
        code: code?.Substring(code.LastIndexOf('.') + 1),
        message: key ?? "",
        statusCode: statusCode,
        details: details);