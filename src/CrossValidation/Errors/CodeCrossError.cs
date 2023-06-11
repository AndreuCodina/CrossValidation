using System.Net;
using System.Runtime.CompilerServices;

namespace CrossValidation.Errors;

public record CodeCrossError(
#pragma warning disable CS8907
    string Code,
#pragma warning restore CS8907
    string? Message = null,
    HttpStatusCode? HttpStatusCode = null,
    string? Details = null,
    [CallerArgumentExpression(nameof(Code))]
    string CodeName = default!) :
    CompleteCrossError(
        Code: CodeName.AsSpan(CodeName.IndexOf('.') + 1)
            .ToString(),
        Message: Message,
        HttpStatusCode: HttpStatusCode,
        Details: Details);