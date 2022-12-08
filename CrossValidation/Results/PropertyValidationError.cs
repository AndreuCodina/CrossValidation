namespace CrossValidation.Results;

public record PropertyValidationError(
    string ErrorCode,
    IEnumerable<object> PlaceholderValues);