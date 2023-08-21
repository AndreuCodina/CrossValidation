using CrossValidation.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CrossValidation.DependencyInjection;

public static class CrossValidationServiceCollectionExtensions
{
    private static IHostEnvironment _environment = default!;
    
    public static IServiceCollection AddCrossValidation(
        this IServiceCollection services,
        Action<CrossValidationOptionsBuilder>? options = null)
    {
        var serviceProvider = services.BuildServiceProvider();
        _environment = serviceProvider.GetRequiredService<IHostEnvironment>();
        
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
                else if (context.Exception is BusinessListException businessListException)
                {
                    var mapper = new ProblemDetailsMapper(context.ProblemDetails);
                    mapper.Map(businessListException);
                    context.HttpContext.Response.StatusCode = (int)context.ProblemDetails.Status!;
                }
                
                if (context.Exception is not null)
                {
                    AddExceptionExtension(context);
                }
            });
        services.AddTransient<GlobalExceptionMiddleware>();
        return services;
    }

    private static void AddExceptionExtension(ProblemDetailsContext context)
    {
        if (!_environment.IsDevelopment())
        {
            return;
        }

        var exceptionExtension = new CrossValidationProblemDetailsException
        {
            Details = context.Exception?.ToString(),
            Headers = context.HttpContext
                .Request
                .Headers
                .ToDictionary(x => x.Key, x => x.Value.ToList()),
            Path = context.HttpContext.Request.Path,
            Endpoint = context.HttpContext.GetEndpoint()?.ToString(),
        };
        context.ProblemDetails.Extensions.Add("exception", exceptionExtension);
    }
}