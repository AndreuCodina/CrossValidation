namespace CrossValidation.Validators;

public abstract class CollectionValidator<TField> : Validator
{
    protected CollectionValidator(IEnumerable<TField> fieldValue)
    {
        TotalItems = fieldValue switch
        {
            ICollection<TField> collection => collection.Count,
            _ => fieldValue.Count()
        };
    }
    
    public int TotalItems { get; private set; }
}