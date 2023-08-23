using CrossValidation.Exceptions;

namespace CrossValidation.Validators.CollectionValidators;

public class UniqueItemsValidator<TField>(IEnumerable<TField> fieldValue) : Validator
{
    public override bool IsValid()
    {
        var totalItems = fieldValue switch
        {
            ICollection<TField> collection => collection.Count,
            _ => fieldValue.Count()
        };
        var totalDistinctItems = fieldValue.Distinct().Count();
        return totalDistinctItems == totalItems;
    }

    public override BusinessException CreateException()
    {
        throw new CommonException.UniqueItemsException();
    }
}