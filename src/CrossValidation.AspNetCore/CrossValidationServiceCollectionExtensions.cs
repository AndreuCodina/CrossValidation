using System.Diagnostics;
using CrossValidation.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CrossValidation.AspNetCore;

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
        AddHttpResponseCustomizers(services);
        return services;
    }

    private static void AddHttpResponseCustomizers(IServiceCollection services)
    {
        if (!CrossValidationOptions.CustomizeHttpResponse)
        {
            return;
        }

        AddProblemDetails(services);
        AddBusinessExceptionMiddleware(services);
    }

    private static void AddBusinessExceptionMiddleware(IServiceCollection services)
    {
        services.AddTransient<BusinessExceptionMiddleware>();
    }

    private static void AddProblemDetails(IServiceCollection services)
    {
        services.AddProblemDetails(problemDetailsOptions =>
            problemDetailsOptions.CustomizeProblemDetails = context =>
            {
                if (context.Exception is BusinessException businessException)
                {
                    AddCodeUrl(businessException, context);
                    var mapper = new ProblemDetailsMapper(context.ProblemDetails);
                    mapper.Map(businessException);
                    context.HttpContext.Response.StatusCode = (int)context.ProblemDetails.Status!;
                }
                else if (context.Exception is BusinessListException businessListException)
                {
                    foreach (var exception in businessListException.Exceptions)
                    {
                        AddCodeUrl(exception, context);
                    }
                    
                    var mapper = new ProblemDetailsMapper(context.ProblemDetails);
                    mapper.Map(businessListException);
                    context.HttpContext.Response.StatusCode = (int)context.ProblemDetails.Status!;
                }

                AddExceptionExtension(context);
                AddTraceIdExtension(context);
            });
    }

    private static void AddTraceIdExtension(ProblemDetailsContext context)
    {
        var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
        context.ProblemDetails
            .Extensions
            .Add(CrossValidationProblemDetails.TraceIdPropertyName, traceId);
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
            Endpoint = context.HttpContext.GetEndpoint()?.ToString()
        };
        context.ProblemDetails
            .Extensions
            .Add(CrossValidationProblemDetails.ExceptionPropertyName, exceptionExtension);
    }
    
    private static void AddCodeUrl(BusinessException exception, ProblemDetailsContext context)
    {
        string? baseUrl = null;
        
        if (!CrossValidationOptions.IsErrorCodePageEnabled || exception.Code is null)
        {
            return;
        }
    
        if (CrossValidationOptions.PublicationUrl is not null)
        {
            baseUrl = CrossValidationOptions.PublicationUrl;
        }
        else if (_environment.IsDevelopment())
        {
            var protocol = context.HttpContext.Request.IsHttps ? "https" : "http";
            var host = context.HttpContext.Request.Host.Value;
            var port = context.HttpContext.Request.Host.Port is not null
                ? $":{context.HttpContext.Request.Host.Port.Value}"
                : null;
            baseUrl = $"{protocol}://{host}{port}";
        }
        else
        {
            return;
        }
        
        var codeUrl = $"{baseUrl}/{CrossValidationOptions.ErrorCodePagePath}#{exception.Code}";
        exception.CodeUrl = codeUrl;
    }
}