using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;

namespace CrossValidation.DependencyInjection;

public static class CrossValidationBuilderExtensions
{
    public static IApplicationBuilder UseCrossValidation(
        this IApplicationBuilder app,
        Action<UseCrossValidationOptionsBuilder>? options = null)
    {
        var builder = new UseCrossValidationOptionsBuilder();
        options?.Invoke(builder);
        return app.UseCustomMiddleware()
            .UseCustomRequestLocalization(builder);
    }
    
    private static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<CrossValidationMiddleware>();
        return app;
    }
    
    private static IApplicationBuilder UseCustomRequestLocalization(
        this IApplicationBuilder app,
        UseCrossValidationOptionsBuilder builder)
    {
        var localizationOptions = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(builder.DefaultCulture),
            SupportedCultures = builder.SupportedCultures,
            SupportedUICultures = builder.SupportedCultures,
            FallBackToParentCultures = true,
            FallBackToParentUICultures = true,
            RequestCultureProviders = new List<IRequestCultureProvider>
            {
                new AcceptLanguageHeaderRequestCultureProvider(),
            }
        };
        app.UseRequestLocalization(localizationOptions);
        return app;
    }
}