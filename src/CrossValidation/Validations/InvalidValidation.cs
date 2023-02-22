namespace CrossValidation.Validations;

public interface IInvalidValidation<out TField> : IValidation<TField>
{
    public static IValidation<TField> Create()
    {
        return new InvalidValidation<TField>();
    }
}

file class InvalidValidation<TField> :
    Validation<TField>,
    IInvalidValidation<TField>
{
    public override TField Instance()
    {
        throw new InvalidOperationException(
            $"Accumulate errors and call {nameof(Instance)} with an invalid validation is not allowed");
    }
    
    public override TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance)
    {
        throw new InvalidOperationException(
            $"Accumulate errors and call {nameof(Instance)} with an invalid validation is not allowed");
    }
}