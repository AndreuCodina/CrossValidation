using CrossValidation.Errors;

namespace CrossValidation.Validators;

public record ErrorPredicateValidator(Func<ICrossError?> Predicate) : Validator
{
    private ICrossError? _error;
    
    public override bool IsValid()
    {
        _error = Predicate();
        return _error is null;
    }

    public override ICrossError CreateError()
    {
        return _error!;
    }
}