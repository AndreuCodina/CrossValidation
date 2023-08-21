namespace CrossValidation.DependencyInjection;

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
}