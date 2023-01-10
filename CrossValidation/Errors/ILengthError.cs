namespace CrossValidation.Errors;

public interface ILengthError : IValidationError
{
    public int TotalLength { get; }
}