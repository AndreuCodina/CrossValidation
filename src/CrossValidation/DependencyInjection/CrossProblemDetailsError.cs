namespace CrossValidation.DependencyInjection;

public class CrossProblemDetailsError
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public Dictionary<string, object>? Placeholders { get; set; }
}