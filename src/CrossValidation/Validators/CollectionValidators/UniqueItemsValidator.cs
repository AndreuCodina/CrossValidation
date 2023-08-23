using CrossValidation.Exceptions;

namespace CrossValidation.Validators.CollectionValidators;

public class UniqueItemsValidator<TField>(IEnumerable<TField> fieldValue)
    : CollectionValidator<TField>(fieldValue)
{
    private readonly IEnumerable<TField> _fieldValue = fieldValue;
    
    public override bool IsValid()
    {
        var totalItems = GetTotalItems();
        var totalDistinctItems = _fieldValue.Distinct()
            .Count();
        return totalDistinctItems == totalItems;
    }

    public override BusinessException CreateException()
    {
        throw new CommonException.UniqueItemsException();
    }
}