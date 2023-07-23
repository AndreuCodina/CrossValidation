namespace CrossValidation.DependencyInjection;

// https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails
public class CrossProblemDetails
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int? Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
    public IEnumerable<CrossProblemDetailsError>? Errors { get; set; }
    public string? ExceptionDetail { get; set; }
}