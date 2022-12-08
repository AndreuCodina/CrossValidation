// using System.Runtime.CompilerServices;
// using CrossValidation.FieldValidators;
// using CrossValidation.Results;
//
// namespace CrossValidation.Rules;
//
// // ConstructorRule, FactoryRule, ValueObjectRule
// public class InlineRule<TField> : IRule<InlineRule<TField>, TField>
// {
//     public TField FieldValue { get; set; }
//
//     // public string FieldName { get; set; }
//     private ValidationError? _validationError;
//
//     public InlineRule(
//         TField fieldValue,
//         [CallerArgumentExpression("fieldValue")]
//         string paramName = "")
//     {
//         FieldValue = fieldValue;
//         _validationError = null;
//     }
//
//     public ValidationError GetOrCreateValidationError()
//     {
//         return _validationError is not null
//             ? _validationError
//             : new ValidationError();
//     }
//
//     public InlineRule<TField> WithMessage(string message)
//     {
//         var validationError = GetOrCreateValidationError();
//         validationError.Message = message;
//         return this;
//     }
//
//     public InlineRule<TField> GetSelf()
//     {
//         return this;
//     }
//
//     public void HandleError(IFieldValidator<TField> validator)
//     {
//         throw new NotImplementedException();
//         // var error = new ValidationError(
//         //     FieldName: "",
//         //     FieldValue: FieldValue,
//         //     ErrorCode: validator.ErrorCode,
//         //     ErrorMessage: validator.ErrorCode
//         // );
//         // throw new ValidationException(new List<ValidationError> {error});
//     }
// }