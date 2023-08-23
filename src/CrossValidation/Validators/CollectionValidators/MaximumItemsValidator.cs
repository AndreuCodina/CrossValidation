using CrossValidation.Exceptions;

namespace CrossValidation.Validators.CollectionValidators;

public class MaximumItemsValidator<TField>(IEnumerable<TField> fieldValue, int maximumItems) : Validator
{
    private int _totalItems;

    public override bool IsValid()
    {
        _totalItems = fieldValue switch
        {
            ICollection<TField> collection => collection.Count,
            _ => fieldValue.Count()
        };
        return _totalItems <= maximumItems;
    }

    public override BusinessException CreateException()
    {
        throw new CommonException.MaximumItemsException(maximumItems, _totalItems);
    }
}