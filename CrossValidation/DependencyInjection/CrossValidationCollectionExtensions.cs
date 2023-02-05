using Microsoft.Extensions.DependencyInjection;

namespace CrossValidation.DependencyInjection;

public static class CrossValidationCollectionExtensions
{
    public static IServiceCollection AddCrossValidation(
        this IServiceCollection services,
        Action<CrossValidationOptions>? options = null)
    {
        var defaultOptions = new CrossValidationOptions();
        options?.Invoke(defaultOptions);
        CrossValidationConfiguration.GeneratePlaceholderValuesWhenTheyAreNotAdded =
            defaultOptions.SendCompleteErrorToClient;
        services.AddTransient<CrossValidationMiddleware>();
        return services;
    }
}