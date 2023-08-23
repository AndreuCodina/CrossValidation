using CrossValidation.Exceptions;

namespace CrossValidation.Validators.CollectionValidators;

public class MaximumItemsValidator<TField>(IEnumerable<TField> fieldValue, int maximumItems)
    : CollectionValidator<TField>(fieldValue)
{
    public override bool IsValid()
    {
        return TotalItems <= maximumItems;
    }

    public override BusinessException CreateException()
    {
        throw new CommonException.MaximumItemsException(maximumItems, TotalItems);
    }
}