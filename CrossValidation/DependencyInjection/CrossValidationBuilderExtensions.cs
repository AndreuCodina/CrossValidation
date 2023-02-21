using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;

namespace CrossValidation.DependencyInjection;

public static class CrossValidationBuilderExtensions
{
    public static IApplicationBuilder UseCrossValidation(this IApplicationBuilder app)
    {
        return app.UseCustomMiddleware()
            .UseCustomRequestLocalization();
    }
    
    private static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<CrossValidationMiddleware>();
        return app;
    }
    
    private static IApplicationBuilder UseCustomRequestLocalization(this IApplicationBuilder app)
    {
        var localizationOptions = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(CrossValidationOptions.DefaultCulture),
            SupportedCultures = CrossValidationOptions.SupportedCultures,
            SupportedUICultures = CrossValidationOptions.SupportedCultures,
            FallBackToParentCultures = true,
            FallBackToParentUICultures = true,
            RequestCultureProviders = new List<IRequestCultureProvider>
            {
                new AcceptLanguageHeaderRequestCultureProvider()
            }
        };
        app.UseRequestLocalization(localizationOptions);
        return app;
    }
}