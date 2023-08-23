using System.Collections;
using System.Globalization;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Localization;

namespace CrossValidation.AspNetCore;

public static class CrossValidationBuilderExtensions
{
    public static IApplicationBuilder UseCrossValidation(this WebApplication app)
    {
        app.UseCustomRequestLocalization();
        // app.UseGlobalExceptionMiddleware();
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        AddErrorCodePage(app);
        return app;
    }

    private static void AddErrorCodePage(WebApplication app)
    {
        if (!CrossValidationOptions.IsErrorCodePageEnabled)
        {
            return;
        }
        
        app.MapGet(CrossValidationOptions.ErrorCodePagePath, CreateErrorCodePage);
    }

    private static ContentHttpResult CreateErrorCodePage()
    {
        var resources = new Dictionary<string, string>();

        foreach (var resourceManager in CrossValidationOptions.ResourceManagers)
        {
            var resourceSet = resourceManager.GetResourceSet(
                culture: new CultureInfo(CrossValidationOptions.DefaultCultureCode),
                createIfNotExists: true,
                tryParents: true)!;

            foreach (DictionaryEntry resource in resourceSet)
            {
                resources.TryAdd(resource.Key.ToString()!, resource.Value!.ToString()!);
            }
        }

        var html = new StringBuilder(
            """
            <!DOCTYPE html>
            <html>
            <head>
                <title>Error codes</title>
            </head>
            <body>
            """);

        foreach (var resource in resources)
        {
            html.AppendLine(
                $$"""
                  <b><a href="#{{resource.Key}}">{{resource.Key}}</a></b>
                  <br>
                  <p><b>Template:</b> {{resource.Value}}</p>
                  <hr>
                  """);
        }

        html.AppendLine(
            """
            </body>
            </html>
            """);
        
        return TypedResults.Content(html.ToString(), MediaTypeNames.Text.Html);
    }

    private static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        return app;
    }
    
    private static IApplicationBuilder UseCustomRequestLocalization(this IApplicationBuilder app)
    {
        var supportedCultures = CrossValidationOptions.SupportedCultureCodes
            .Select(x => new CultureInfo(x))
            .ToList();
        var localizationOptions = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(CrossValidationOptions.DefaultCultureCode),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures,
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