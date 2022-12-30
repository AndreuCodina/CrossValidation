using CrossValidation.Results;
using CrossValidation.ValidationContexts;
using CrossValidation.Validators;

namespace CrossValidation.Rules;

public interface IRule<out TField>
{
    bool IsValid { get;set; }
    ValidationContext Context { get; set; }
    string FieldFullPath { get; set; }
    TField GetFieldValue();
    IRule<TField> SetValidator(Func<Validator> validator);
    IRule<TFieldTransformed> Transform<TFieldTransformed>(
        Func<TField, TFieldTransformed> transformer);
    IRule<TField> WithMessage(string message);
    IRule<TField> WithCode(string code);
    IRule<TField> WithError(CrossValidationError error);
    IRule<TField> WithFieldDisplayName(string fieldDisplayName);
    IRule<TField> When(bool condition);
    IRule<TField> When(Func<TField, bool> condition);
    IRule<TField> Must(Func<TField, bool> condition);
    TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance);
    IRule<TField> SetModelValidator<TChildModel>(ModelValidator<TChildModel> validator);
}