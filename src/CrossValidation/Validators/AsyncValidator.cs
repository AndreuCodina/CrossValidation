// using CrossValidation.Errors;
//
// namespace CrossValidation.Validators;
//
// public interface IAsyncValidator<out TValidationError>
// {
//     public Task<bool> IsValid();
//     public TValidationError CreateError();
//     public TValidationError? GetError();
// }
//
// public abstract record AsyncValidator : Validator<ICrossError>;
//
// public abstract record AsyncValidator<TValidationError> : IValidator<TValidationError>
//     where TValidationError : class, ICrossError
// {
//     public abstract Task<bool> IsValid();
//     
//     public abstract TValidationError CreateError();
//     
//     public TValidationError? GetError()
//     {
//         return !IsValid() ? (TValidationError?)CreateError() : null;
//     }
// }