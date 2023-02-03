﻿using CrossValidation.Errors;

namespace CrossValidation.Validators.LengthValidators;

public record LengthRangeValidator(string FieldValue, int Minimum, int Maximum) : LengthValidatorBase
{
    public override bool IsValid()
    {
        return FieldValue.Length >= Minimum
               && FieldValue.Length <= Maximum;
    }

    public override ILengthError CreateError()
    {
        return new CommonCodeValidationError.LengthRange(Minimum, Maximum, GetTotalLength(FieldValue));
    }
}