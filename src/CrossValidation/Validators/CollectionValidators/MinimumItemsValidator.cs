using CrossValidation.Exceptions;

namespace CrossValidation.Validators.CollectionValidators;

public class MinimumItemsValidator<TField>(IEnumerable<TField> fieldValue, int minimumItems) : Validator
{
    private int _totalItems;

    public override bool IsValid()
    {
        _totalItems = fieldValue switch
        {
            ICollection<TField> collection => collection.Count,
            _ => fieldValue.Count()
        };
        return _totalItems >= minimumItems;
    }

    public override BusinessException CreateException()
    {
        throw new CommonException.MinimumItemsException(minimumItems, _totalItems);
    }
}