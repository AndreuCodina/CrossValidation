namespace CrossValidation.AspNetCore;

// https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails
public class CrossValidationProblemDetails
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int? Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
    public IEnumerable<CrossValidationProblemDetailsError>? Errors { get; set; }
    public CrossValidationProblemDetailsException? Exception { get; set; }
    public required string TraceId { get; set; }

    public static string ErrorsPropertyName = GetPropertyName(nameof(Errors));
    public static string ExceptionPropertyName = GetPropertyName(nameof(Exception));
    public static string TraceIdPropertyName = GetPropertyName(nameof(TraceId));

    private static string GetPropertyName(string originalPropertyName)
    {
        return originalPropertyName[..1].ToLowerInvariant() + originalPropertyName[1..];
    }
}