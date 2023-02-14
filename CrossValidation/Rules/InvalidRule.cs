namespace CrossValidation.Rules;

public interface IInvalidRule<out TField> : IRule<TField>
{
    public static IRule<TField> Create()
    {
        return new InvalidRule<TField>();
    }
}

file class InvalidRule<TField> :
    Rule<TField>,
    IInvalidRule<TField>
{
    public override TField Instance()
    {
        throw new InvalidOperationException(
            $"Accumulate errors and call {nameof(Instance)} with an invalid rule is not allowed");
    }
    
    public override TInstance Instance<TInstance>(Func<TField, TInstance> fieldToInstance)
    {
        throw new InvalidOperationException(
            $"Accumulate errors and call {nameof(Instance)} with an invalid rule is not allowed");
    }
}