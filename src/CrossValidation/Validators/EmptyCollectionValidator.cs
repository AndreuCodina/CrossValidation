using CrossValidation.Exceptions;

namespace CrossValidation.Validators;

public class EmptyCollectionValidator<TField>(IEnumerable<TField> fieldValue) : Validator
{
    public override bool IsValid()
    {
        return !fieldValue.Any();
    }

    public override BusinessException CreateException()
    {
        return new CommonException.EmptyCollection();
    }
}