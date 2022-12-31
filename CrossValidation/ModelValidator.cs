using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using CrossValidation.Exceptions;
using CrossValidation.Rules;
using CrossValidation.ValidationContexts;

namespace CrossValidation;

public abstract record ModelValidator<TModel>
{
    private ValidationMode _validationMode = ValidationMode.StopOnFirstError;
    public ValidationContext? Context { get; set; }
    public TModel? Model { get; set; }

    public ValidationMode ValidationMode
    {
        get => _validationMode;
        set
        {
            if (Context!.IsChildContext)
            {
                throw new InvalidOperationException("Cannot change the validation mode in a child model validator");
            }
            
            _validationMode = value;
            Context!.ValidationMode = _validationMode;
        }
    }

    public IRule<TField> RuleFor<TField>(Expression<Func<TModel, TField>> fieldSelector)
    {
        return Rule<TField>.CreateFromFieldSelector(Model!, fieldSelector, Context!);
    }

    public abstract void CreateRules(TModel model);

    public void Validate(TModel model)
    {
        if (Context is not {IsChildContext: true})
        {
            ValidateNullability(model);
            Context = new ValidationContext();
        }

        Model = model;
        CreateRules(model);

        if (!Context.IsChildContext && Context.Errors is not null)
        {
            throw new CrossValidationException(Context.Errors!);
        }
    }

    /// <summary>
    /// Check model and nested models don't have nulls in non-nullable types
    /// </summary>
    /// <param name="model"></param>
    /// <exception cref="ArgumentNullException"></exception>
    private void ValidateNullability(TModel model)
    {
        ValidateModel(model!);
    }

    private void ValidateModel(object model)
    {
        var nullabilityContext = new NullabilityInfoContext();
        
        foreach (var property in model!.GetType().GetProperties())
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

    private void ValidateNestedModel(PropertyInfo property, object model)
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
            throw new ModelFormatException($"Non-nullable type {property.Name} is null");
        }
    }

    private void ValidateCollectionProperty(
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
                    throw new ModelFormatException($"The collection '{property.Name}' cannot contain null values");
                }
            }
        }
    }
}