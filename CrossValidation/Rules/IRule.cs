using CrossValidation.Errors;
using CrossValidation.ValidationContexts;
using CrossValidation.Validators;

namespace CrossValidation.Rules;

public interface IRule<out TField>
{
    RuleState State { get;set; }
    ValidationContext Context { get; set; }
    string FieldFullPath { get; set; }
    TField GetFieldValue();
    IRule<TField> SetValidator(Func<IValidator<ICrossValidationError>> validator);
    IRule<TFieldTransformed> Transform<TFieldTransformed>(
        Func<TField, TFieldTransformed> transformer);
    IRule<TField> WithMessage(string message);
    IRule<TField> WithCode(string code);
    IRule<TField> WithError(CrossValidationError error);
    IRule<TField> WithFieldDisplayName(string fieldDisplayName);
    IRule<TField> When(bool condition);
    IRule<TField> When(Func<TField, bool> condition);
    IRule<TField> WhenAsync(Func<TField, Task<bool>> condition);
    IRule<TField> Must(Func<TField, bool> condition);
    IRule<TField> MustAsync(Func<TField, Task<bool>> condition);
    TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance);
    IRule<TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator);
}