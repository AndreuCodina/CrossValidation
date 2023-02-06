using System.Collections;
using System.Reflection;
using CrossValidation.Errors;
using static CrossValidation.Utils.ModelNullabilityValidatorError;

namespace CrossValidation.Utils;

public record ModelNullabilityValidatorError : EnsureError
{
    public record NonNullablePropertyIsNullError(string PropertyName) : ModelNullabilityValidatorError;
    public record NonNullableItemCollectionWithNullItemError(string CollectionName) : ModelNullabilityValidatorError;
}

internal static class ModelNullabilityValidator
{
    /// <summary>
    /// Check model and nested models don't have nulls in non-nullable types
    /// </summary>
    public static void Validate<TModel>(TModel model)
    {
        ValidateModel(model!);
    }

    private static void ValidateModel(object model)
    {
        var nullabilityContext = new NullabilityInfoContext();

        foreach (var property in model.GetType().GetProperties())
        {
            var propertyValue = property.GetValue(model);
            var nullabilityInfo = nullabilityContext.Create(property);

            if (propertyValue is null)
            {
                ValidatePropertyIsNullable(nullabilityInfo, property);
            }
            else
            {
                ValidateCollectionProperty(nullabilityContext, property, model);
                ValidateNestedModel(property, model);
            }
        }
    }

    private static void ValidateNestedModel(PropertyInfo property, object model)
    {
        var isNestedModel = property.PropertyType.IsNested;

        if (isNestedModel)
        {
            ValidateModel(model);
        }
    }

    private static void ValidatePropertyIsNullable(NullabilityInfo nullabilityInfo, PropertyInfo property)
    {
        var isNonNullableProperty = nullabilityInfo.WriteState is not NullabilityState.Nullable;

        if (isNonNullableProperty)
        {
            throw new NonNullablePropertyIsNullError(property.Name).ToException();
        }
    }

    private static void ValidateCollectionProperty(
        NullabilityInfoContext nullabilityContext,
        PropertyInfo property,
        object model)
    {
        var isCollection = typeof(IEnumerable).IsAssignableFrom(property.PropertyType)
                           && property.PropertyType != typeof(string);

        if (isCollection)
        {
            var collection = (IEnumerable)property.GetValue(model)!;
            var nullabilityInfo = nullabilityContext.Create(property);

            foreach (var item in collection)
            {
                if (item is null &&
                    nullabilityInfo.GenericTypeArguments[0].WriteState is not NullabilityState.Nullable)
                {
                    throw new NonNullableItemCollectionWithNullItemError(property.Name).ToException();
                }
            }
        }
    }
}