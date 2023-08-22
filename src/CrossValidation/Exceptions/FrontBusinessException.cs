using System.Net;

namespace CrossValidation.Exceptions;

/// <summary>
/// Generates placeholders to return to the frontend
/// </summary>
// TODO: Create source generator to generate placeholders
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