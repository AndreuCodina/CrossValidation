using CrossValidation.Exceptions;

namespace CrossValidation.Validators.CollectionValidators;

public class MinimumItemsValidator<TField>(IEnumerable<TField> fieldValue, int minimumItems)
    : CollectionValidator<TField>(fieldValue)
{
    public override bool IsValid()
    {
        return TotalItems >= minimumItems;
    }

    public override BusinessException CreateException()
    {
        throw new CommonException.MinimumItemsException(minimumItems, TotalItems);
    }
}