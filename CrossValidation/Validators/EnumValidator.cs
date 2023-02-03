﻿using CrossValidation.Errors;

namespace CrossValidation.Validators;

public record EnumValidator<TField>(
    TField FieldValue,
    Type enumType) : Validator
{
    public override bool IsValid()
    {
        return Enum.IsDefined(enumType, FieldValue!);
    }

    public override ValidationError CreateError()
    {
        return new CommonCodeValidationError.Enum();
    }
}