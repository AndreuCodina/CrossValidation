namespace CrossValidation;

public enum ValidationMode
{
    StopOnFirstError,
    AccumulateFirstError, // TODO: Rename to AccumulateFirstErrors
        // .Field(model.Preferences)
        //   .SetModelValidator(myModelValidator);
        // Return all errors, because ModelValidator executes all validations
        
        // .Field(model.Preferences)
        //   .WhenNotNull(x => x
        //     .MustAsync(false) // Will be marked as HasFailed=true, 
        //     .SetModelValidator(myModelValidator)
        //     .Must(() => false));
        // Return all errors, because ModelValidator executes all validations
            // When the first validation failure occurs in a ModelValidator field, we notify parent scope creators
            // The ModelValidator will continue executing the rest of its fields
            // --
            // In a sync execution, Must() will not be executed because the scope ModelValidator failed and the next validation (Must) will be created with HasFailed=true
            // In an async execution, Must() will not be executed because the previous validation (the ModelValidator scope) has been marked with HasFailed=true
            // --
            // 

            // .Field(model.Preferences)
        //   .ForEach(x => x
        //     .SetModelValidator(myModelValidator));
        // Return all errors of first ModelValidator failure, and stop to validate after it
    AccumulateFirstErrorAndAllFirstErrorsCollectionIteration // TODO: Rename to AccumulateFirstErrorsAndAllFirstErrorsCollectionIteration -> AccumulateAllErrors
}