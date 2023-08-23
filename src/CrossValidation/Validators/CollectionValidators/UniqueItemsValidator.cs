using CrossValidation.Exceptions;

namespace CrossValidation.Validators.CollectionValidators;

public class UniqueItemsValidator<TField>(IEnumerable<TField> fieldValue)
    : CollectionValidator<TField>(fieldValue)
{
    public override bool IsValid()
    {
        var totalDistinctItems = fieldValue.Distinct().Count();
        return totalDistinctItems == TotalItems;
    }

    public override BusinessException CreateException()
    {
        throw new CommonException.UniqueItemsException();
    }
}