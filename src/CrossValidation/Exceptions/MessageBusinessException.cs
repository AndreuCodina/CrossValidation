using System.Net;

namespace CrossValidation.Exceptions;

public class MessageBusinessException(
    string message = "",
    HttpStatusCode statusCode = HttpStatusCode.UnprocessableEntity,
    string? details = null,
    int statusCodeInt = (int)HttpStatusCode.UnprocessableEntity)
    : BusinessException(
        message: message,
        statusCode: statusCode,
        details: details,
        statusCodeInt: statusCodeInt);