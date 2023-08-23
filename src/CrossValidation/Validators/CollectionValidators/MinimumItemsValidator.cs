using CrossValidation.Exceptions;

namespace CrossValidation.Validators.CollectionValidators;

public class MinimumItemsValidator<TField>(IEnumerable<TField> fieldValue, int minimumItems)
    : CollectionValidator<TField>(fieldValue)
{
    private int _totalItems;

    public override bool IsValid()
    {
        _totalItems = GetTotalItems();
        return _totalItems >= minimumItems;
    }

    public override BusinessException CreateException()
    {
        throw new CommonException.MinimumItemsException(minimumItems, _totalItems);
    }
}