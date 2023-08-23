using CrossValidation.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CrossValidation.AspNetCore.DependencyInjection;

public static class BusinessExceptionExtensions
{
    public static ProblemDetails ToProblemDetails(this BusinessException exception)
    {
        var problemDetails = new ProblemDetails();
        var mapper = new ProblemDetailsMapper(problemDetails);
        mapper.Map(exception);
        return problemDetails;
    }
    
    public static ProblemDetails ToProblemDetails(this BusinessListException exception)
    {
        var problemDetails = new ProblemDetails();
        var mapper = new ProblemDetailsMapper(problemDetails);
        mapper.Map(exception);
        return problemDetails;
    }
}