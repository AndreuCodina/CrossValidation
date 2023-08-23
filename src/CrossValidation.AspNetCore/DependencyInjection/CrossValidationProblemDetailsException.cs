namespace CrossValidation.AspNetCore.DependencyInjection;

public class CrossValidationProblemDetailsException
{
    public required string? Details { get; set; }
    public required Dictionary<string, List<string?>>? Headers { get; set; }
    public required string? Path { get; set; }
    public required string? Endpoint { get; set; }
}