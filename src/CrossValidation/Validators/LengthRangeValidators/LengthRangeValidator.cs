namespace CrossValidation.Validators.LengthRangeValidators;

public abstract class LengthRangeValidator : Validator
{
    protected LengthRangeValidator(int minimum, int maximum)
    {
        EnsureParameterPreconditions(minimum, maximum);
    }
    
    private void EnsureParameterPreconditions(int minimum, int maximum)
    {
        if (minimum < 0)
        {
            throw new ArgumentException("The minimum length cannot be less than zero");
        }
        else if (maximum < 0)
        {
            throw new ArgumentException("The maximum length cannot be less than zero");
        }
        else if (minimum > maximum)
        {
            throw new ArgumentException("The minimum length cannot be greater than the maximum length");
        }
    }
}