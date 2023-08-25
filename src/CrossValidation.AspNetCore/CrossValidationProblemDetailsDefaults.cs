using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace CrossValidation.AspNetCore;

internal static class CrossValidationProblemDetailsDefaults
{
    public static readonly Dictionary<int, (string Type, string Title)> Defaults = new()
    {
        [400] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            "Bad Request"
        ),

        [401] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            "Unauthorized"
        ),

        [403] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            "Forbidden"
        ),

        [404] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            "Not Found"
        ),

        [405] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.6",
            "Method Not Allowed"
        ),

        [406] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.7",
            "Not Acceptable"
        ),

        [408] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.9",
            "Request Timeout"
        ),

        [409] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            "Conflict"
        ),

        [412] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.13",
            "Precondition Failed"
        ),

        [415] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.16",
            "Unsupported Media Type"
        ),

        [422] =
        (
            "https://tools.ietf.org/html/rfc4918#section-11.2",
            "Unprocessable Entity"
        ),

        [426] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.5.22",
            "Upgrade Required"
        ),

        [500] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            "An error occurred while processing your request."
        ),

        [502] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.6.3",
            "Bad Gateway"
        ),

        [503] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.6.4",
            "Service Unavailable"
        ),

        [504] =
        (
            "https://tools.ietf.org/html/rfc9110#section-15.6.5",
            "Gateway Timeout"
        ),
    };

    public static void Apply(ProblemDetails problemDetails, int statusCode)
    {
        problemDetails.Title = null;
        problemDetails.Type = null;
        problemDetails.Status = statusCode;
        
        if (Defaults.TryGetValue(statusCode, out var defaults))
        {
            problemDetails.Title ??= defaults.Title;
            problemDetails.Type ??= defaults.Type;
        }
        
        var reasonPhrase = ReasonPhrases.GetReasonPhrase(statusCode);
        if (!string.IsNullOrEmpty(reasonPhrase))
        {
            problemDetails.Title = reasonPhrase;
        }
    }
}