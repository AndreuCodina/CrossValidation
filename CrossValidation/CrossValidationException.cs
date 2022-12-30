﻿using CrossValidation.Results;

namespace CrossValidation;

public class CrossValidationException : Exception
{
    public List<CrossValidationError> Errors { get; }

    public CrossValidationException(List<CrossValidationError> errors)
    {
        Errors = errors;
    }
}