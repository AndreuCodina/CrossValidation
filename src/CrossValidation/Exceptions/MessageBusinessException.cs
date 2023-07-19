using System.Net;

namespace CrossValidation.Exceptions;

public class MessageBusinessException(
    string message,
    HttpStatusCode statusCode = HttpStatusCode.UnprocessableEntity,
    string? details = null)
    : BusinessException(
        message: message,
        statusCode: statusCode,
        details: details);