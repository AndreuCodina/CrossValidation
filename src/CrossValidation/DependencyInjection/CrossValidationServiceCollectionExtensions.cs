using Microsoft.Extensions.DependencyInjection;

namespace CrossValidation.DependencyInjection;

public static class CrossValidationServiceCollectionExtensions
{
    public static IServiceCollection AddCrossValidation(
        this IServiceCollection services,
        Action<CrossValidationOptionsBuilder>? options = null)
    {
        options?.Invoke(new CrossValidationOptionsBuilder());
        services.AddTransient<CrossValidationMiddleware>();
        return services;
    }
}