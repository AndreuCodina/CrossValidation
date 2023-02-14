﻿using CrossValidation.Errors;

namespace CrossValidation.Validators;

public record EnumValidator<TField>(
    TField FieldValue,
    Type EnumType) : Validator
{
    public override bool IsValid()
    {
        return Enum.IsDefined(EnumType, FieldValue!);
    }

    public override CrossError CreateError()
    {
        return new CommonCrossError.Enum();
    }
}