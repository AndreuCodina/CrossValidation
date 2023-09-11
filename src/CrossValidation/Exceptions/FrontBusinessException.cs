using System.Net;

namespace CrossValidation.Exceptions;

/// <summary>
/// Generates placeholders to return to the frontend
/// </summary>
public class FrontBusinessException(
    string? code = null,
    HttpStatusCode statusCode = HttpStatusCode.UnprocessableEntity,
    string? details = null,
    int statusCodeInt = (int)HttpStatusCode.UnprocessableEntity)
    : BusinessException(
        code: code,
        statusCode: statusCode,
        details: details,
        statusCodeInt: statusCodeInt);