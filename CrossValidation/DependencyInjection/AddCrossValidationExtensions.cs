using Microsoft.Extensions.DependencyInjection;

namespace CrossValidation.DependencyInjection;

public static class CrossValidationCollectionExtensions
{
    public static IServiceCollection AddCrossValidation(
        this IServiceCollection services,
        Action<AddCrossValidationOptionsBuilder>? options = null)
    {
        options?.Invoke(new AddCrossValidationOptionsBuilder());
        services.AddTransient<CrossValidationMiddleware>();
        return services;
    }
}