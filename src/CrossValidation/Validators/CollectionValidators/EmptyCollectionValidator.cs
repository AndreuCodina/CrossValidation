using CrossValidation.Exceptions;

namespace CrossValidation.Validators.CollectionValidators;

public class EmptyCollectionValidator<TField>(IEnumerable<TField> fieldValue)
    : CollectionValidator<TField>(fieldValue)
{
    private readonly IEnumerable<TField> _fieldValue = fieldValue;
    
    public override bool IsValid()
    {
        return !_fieldValue.Any();
    }

    public override BusinessException CreateException()
    {
        return new CommonException.EmptyCollectionException();
    }
}