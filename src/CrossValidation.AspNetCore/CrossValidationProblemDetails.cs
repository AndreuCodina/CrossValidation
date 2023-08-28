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

    public static string ErrorsPropertyName = nameof(Errors).ToLowerInvariant();
    public static string ExceptionPropertyName = nameof(Exception).ToLowerInvariant();
    public static string TraceIdPropertyName = nameof(TraceId).ToLowerInvariant();
}