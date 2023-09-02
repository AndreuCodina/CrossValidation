using CrossValidation.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace CrossValidation.AspNetCore;

internal class BusinessExceptionMiddleware(IProblemDetailsService problemDetailsService) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            if (context.Response.HasStarted
                || e is not BusinessException or BusinessListException)
            {
                throw;
            }
            
            await HandleBusinessException(context, e);
        }
    }

    private async Task HandleBusinessException(HttpContext context, Exception exception)
    {
        ClearHttpContext(context);
        PreventCacheErrorResponse(context);
        await WriteResponse(problemDetailsService, context, exception);
    }

    private async Task WriteResponse(IProblemDetailsService problemDetailsService, HttpContext context, Exception exception)
    {
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
        });
    }

    private void ClearHttpContext(HttpContext context)
    {
        context.Response.Clear();
    }
    
    private void PreventCacheErrorResponse(HttpContext context)
    {
        context.Response.OnStarting(ClearCacheHeaders, context.Response);
    }
    
    private Task ClearCacheHeaders(object state)
    {
        var headers = ((HttpResponse)state).Headers;
        headers.CacheControl = "no-cache,no-store";
        headers.Pragma = "no-cache";
        headers.Expires = "-1";
        headers.ETag = default;
        return Task.CompletedTask;
    }
}