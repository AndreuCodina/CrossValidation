using System.Net;
using System.Runtime.CompilerServices;
using CrossValidation.Exceptions;

namespace CrossValidation.Errors;

public record CodeCrossError(
#pragma warning disable CS8907
    string Code,
#pragma warning restore CS8907
    string? Message = null,
    HttpStatusCode? HttpStatusCode = null,
    string? Details = null,
    [CallerArgumentExpression(nameof(Code))]
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string CodeName = default!) :
    BusinessException(
        Code: CodeName,
        Message: Message,
        HttpStatusCode: HttpStatusCode,
        Details: Details);