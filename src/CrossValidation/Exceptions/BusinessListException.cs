namespace CrossValidation.Exceptions;

public class BusinessListException(List<BusinessException> exceptions) : Exception
{
    public List<BusinessException> Exceptions => exceptions;
}