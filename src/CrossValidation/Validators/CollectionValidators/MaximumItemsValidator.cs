using CrossValidation.Exceptions;

namespace CrossValidation.Validators.CollectionValidators;

public class MaximumItemsValidator<TField>(IEnumerable<TField> fieldValue, int maximumItems)
    : CollectionValidator<TField>(fieldValue)
{
    private int _totalItems;

    public override bool IsValid()
    {
        _totalItems = GetTotalItems();
        return _totalItems <= maximumItems;
    }

    public override BusinessException CreateException()
    {
        throw new CommonException.MaximumItemsException(maximumItems, _totalItems);
    }
}