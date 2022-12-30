namespace CrossValidation.Utils;

public record FieldInformation<TField>(
    string SelectionFullPath,
    TField Value);