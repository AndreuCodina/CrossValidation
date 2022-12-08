namespace CrossValidation.Results;

// public record ValidationError(
//     string? FieldName = null, // string? or ""
//     object? FieldValue = null,
//     string? ErrorCode = null,
//     string? ErrorMessage = null);


public interface IErrorCode{}
public interface IErrorMessage{}

public record ValidationError(
    string? FieldName = null,
    object? FieldValue = null,
    string? Code = null,
    string? Message = null,
    string? Detail = null,
    object[]? Parameters = null);
// public object? FieldValue { get; set; }