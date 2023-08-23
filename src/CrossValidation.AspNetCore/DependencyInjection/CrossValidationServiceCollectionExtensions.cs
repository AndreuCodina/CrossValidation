using CrossValidation.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CrossValidation.AspNetCore.DependencyInjection;

public static class CrossValidationServiceCollectionExtensions
{
    private const string TraceIdHeaderKey = "X-Trace-Id";
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


                AddExceptionExtension(context);
                AddTraceIdExtension(context);
            });
        services.AddTransient<GlobalExceptionMiddleware>();
        return services;
    }

    private static void AddTraceIdExtension(ProblemDetailsContext context)
    {
        var hasTraceIdHeaders = context.HttpContext.Request.Headers.TryGetValue(
            TraceIdHeaderKey, out var correlationIds);
        var traceId = hasTraceIdHeaders
            ? correlationIds.FirstOrDefault()
            : Guid.NewGuid().ToString();
        context.HttpContext
            .Response
            .Headers
            .Append(TraceIdHeaderKey, traceId);
        context.ProblemDetails.Extensions.Add("traceId", traceId);
    }

    private static void AddExceptionExtension(ProblemDetailsContext context)
    {
        if (context.Exception is null || !_environment.IsDevelopment())
        {
            return;
        }

        var exceptionExtension = new CrossValidationProblemDetailsException
        {
            Details = context.Exception.ToString(),
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