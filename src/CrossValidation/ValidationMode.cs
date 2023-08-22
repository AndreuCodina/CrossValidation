namespace CrossValidation;

public enum ValidationMode
{
    StopOnFirstError,
    AccumulateFirstErrorRelatedToField,
    AccumulateFirstErrorRelatedToFieldAndFirstErrorOfAllIterations
}