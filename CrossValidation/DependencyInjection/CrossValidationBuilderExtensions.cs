using Microsoft.AspNetCore.Builder;

namespace CrossValidation.DependencyInjection;

public static class CrossValidationBuilderExtensions
{
    public static IApplicationBuilder UseCrossValidation(this IApplicationBuilder app)
    {
        app.UseMiddleware<CrossValidationMiddleware>();
        return app;
    }
}