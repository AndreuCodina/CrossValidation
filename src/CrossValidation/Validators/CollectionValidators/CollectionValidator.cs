namespace CrossValidation.Validators.CollectionValidators;

public abstract class CollectionValidator<TField> : Validator
{
    private readonly IEnumerable<TField> _fieldValue;

    protected CollectionValidator(IEnumerable<TField> fieldValue)
    {
        _fieldValue = fieldValue;
    }

    public int GetTotalItems()
    {
        return _fieldValue switch
        {
            ICollection<TField> collection => collection.Count,
            _ => _fieldValue.Count()
        };
    }
}