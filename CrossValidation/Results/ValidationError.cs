namespace CrossValidation.Results;

public interface IErrorCode{}
public interface IErrorMessage{}

public record ValidationError(
    string? FieldName = null,
    object? FieldValue = null,
    string? Code = null,
    string? Message = null,
    string? Detail = null,
    object[]? Parameters = null);