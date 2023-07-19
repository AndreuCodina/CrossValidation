namespace CrossValidation.Exceptions;

public class ValidationListException(List<BusinessException> errors) : Exception
{
    public List<BusinessException> Exceptions => errors;
}