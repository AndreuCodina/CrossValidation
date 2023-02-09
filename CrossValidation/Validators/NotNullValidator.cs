﻿using CrossValidation.Errors;

namespace CrossValidation.Validators;

public record NotNullValidator<TField>(TField? FieldValue) : Validator
{
    public override bool IsValid()
    {
        return FieldValue is not null;
    }

    public override CrossError CreateError()
    {
        return new CommonCrossError.NotNull();
    }
}