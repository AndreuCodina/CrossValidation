namespace CrossValidation.DependencyInjection;

public class CrossProblemDetailsError
{
    public required string? Code { get; set; }
    public required string? CodeUrl { get; set; }
    public required string? Message { get; set; }
    public required string? Detail { get; set; }
    public required Dictionary<string, object?>? Placeholders { get; set; }
}