namespace CrossValidation.AspNetCore;

public class CrossValidationProblemDetailsError
{
    public required string? Code { get; set; }
    public required string? CodeUrl { get; set; }
    public required string? Message { get; set; }
    public required string? Details { get; set; }
    public required string? FieldName { get; set; }
    public required Dictionary<string, object?>? Placeholders { get; set; }
}