namespace CrossValidation;

public enum ValidationMode
{
    StopValidationOnFirstError = 1,
    AccumulateFirstErrorEachValidation = 2, // TODO: Rename to AccumulateFirstError
    AccumulateFirstErrorEachValidationAndAllFirstErrorsCollectionIteration // TODO: Rename to AccumulateFirstErrorAndAllFirstErrorsCollectionIteration
}