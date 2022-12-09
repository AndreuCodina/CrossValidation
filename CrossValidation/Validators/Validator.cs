using CrossValidation.Results;

namespace CrossValidation.Validators;

public abstract record Validator
{
    public ValidationError? Error { get; set; }
    public abstract bool IsValid();
    public abstract ValidationError CreateError();
    
    public bool HasError()
    {
        var isValid = IsValid();
        
        if (!isValid)
        {
            Error = CreateError();
            return true;
        }

        return false;
    }
}