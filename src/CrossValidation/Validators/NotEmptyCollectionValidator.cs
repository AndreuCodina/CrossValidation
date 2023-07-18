using CrossValidation.Errors;
using CrossValidation.Exceptions;

namespace CrossValidation.Validators;

public class NotEmptyCollectionValidator<TField>(IEnumerable<TField> fieldValue) : Validator
{
    public override bool IsValid()
    {
        return fieldValue.Any();
    }

    public override BusinessException CreateError()
    {
        return new CommonCrossError.NotEmptyCollection();
    }
}