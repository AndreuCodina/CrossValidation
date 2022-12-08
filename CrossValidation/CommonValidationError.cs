using CrossValidation.Results;

namespace CrossValidation;

public abstract record CommonValidationError(string Code) : ValidationError(Code: Code)
{
    public record NotNull() : CommonValidationError("NotNull");
    public record Null() : CommonValidationError("Null");
    public record GreaterThan<T>(T ComparisonValue) : CommonValidationError("GreaterThan");
}