﻿using CrossValidation.Results;

namespace CrossValidation;

public abstract record CommonValidationError(string Code) : ValidationError(Code: Code)
{
    public record NotNull() : CommonValidationError("NotNull");
    public record Null() : CommonValidationError("Null");
    public record GreaterThan<T>(T ComparisonValue) : CommonValidationError("GreaterThan");
    public record Enum() : CommonValidationError("Enum");
    public record Length(int Minimum, int Maximum) : CommonValidationError("Length");
}