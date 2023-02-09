namespace CrossValidation.Errors;

public interface ILengthError : ICrossError
{
    public int TotalLength { get; }
}