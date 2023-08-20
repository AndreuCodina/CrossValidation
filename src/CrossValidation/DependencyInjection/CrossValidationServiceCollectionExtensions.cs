using CrossValidation.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace CrossValidation.DependencyInjection;

public static class CrossValidationServiceCollectionExtensions
{
    public static IServiceCollection AddCrossValidation(
        this IServiceCollection services,
        Action<CrossValidationOptionsBuilder>? options = null)
    {
        options?.Invoke(new CrossValidationOptionsBuilder());
        services.AddProblemDetails(problemDetailsOptions =>
            problemDetailsOptions.CustomizeProblemDetails = context =>
            {
                if (context.Exception is BusinessException businessException)
                {
                    var mapper = new ProblemDetailsMapper(context.ProblemDetails);
                    mapper.Map(businessException);
                    context.HttpContext.Response.StatusCode = (int)context.ProblemDetails.Status!;
                }

                if (context.Exception is BusinessListException businessListException)
                {
                    var mapper = new ProblemDetailsMapper(context.ProblemDetails);
                    mapper.Map(businessListException);
                    context.HttpContext.Response.StatusCode = (int)context.ProblemDetails.Status!;
                }
            });
        services.AddTransient<GlobalExceptionMiddleware>();
        return services;
    }
}