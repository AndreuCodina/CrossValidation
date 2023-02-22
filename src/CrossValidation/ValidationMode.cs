namespace CrossValidation;

public enum ValidationMode
{
    StopValidationOnFirstError = 1,
    AccumulateFirstErrorEachValidation = 2,
    AccumulateFirstErrorEachValidationAndAllFirstErrorsCollectionIteration
}