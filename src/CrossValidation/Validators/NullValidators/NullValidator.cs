﻿using CrossValidation.Exceptions;

namespace CrossValidation.Validators.NullValidators;

public class NullValidator<TField>(TField? fieldValue) : Validator
{
    public override bool IsValid()
    {
        return fieldValue is null;
    }

    public override BusinessException CreateException()
    {
        return new CommonException.NullException();
    }
}