namespace CrossValidationTests.Models;

public class NestedModel
{
    public required string String { get; set; }
    public required int Int { get; set; }
    public required NestedNestedModel NestedNestedModel { get; set; }
}