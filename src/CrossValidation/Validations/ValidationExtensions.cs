using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using CrossValidation.Validators;
using CrossValidation.Validators.LengthValidators;

namespace CrossValidation.Validations;

public static class ValidationExtensions
{
    public static IValidation<TField> NotNull<TField>(
        this IValidation<TField?> validation)
        where TField : class
    {

        return validation.SetValidator(() => new NotNullValidator<TField?>(validation.GetFieldValue()))
            .Transform(x => x!);
    }

    public static IValidation<TField> NotNull<TField>(
        this IValidation<TField?> validation)
        where TField : struct
    {
        return validation.SetValidator(() => new NotNullValidator<TField?>(validation.GetFieldValue()))
            .Transform(x => x!.Value);
    }

    public static IValidation<TField?> WhenNotNull<TField, TReturnedField>(
        this IValidation<TField?> validation,
        Func<IValidation<TField>, IValidation<TReturnedField>> notNullValidation)
        where TField : struct
    {
        if (validation.HasFailed)
        {
            return validation;
        }
        
        return validation.SetValidationScope(() =>
        {
            if (validation.GetFieldValue() is not null)
            {
 
                
                // if (validationReturned.HasFailed)
                // {
                //     return false;
                // }
            }

            return true;
        });

        // if (validation.GetFieldValue() is not null)
        // {
        //     var validationReturned = notNullValidation(validation.Transform(x => x!.Value));
        //
        //     if (validationReturned is IInvalidation<TReturnedField>)
        //     {
        //         return IInvalidation<TField?>.Create();
        //     }
        // }

        // return validation;
    }

    public static IValidation<TField?> WhenNotNull<TField, TReturnedField>(
        this IValidation<TField?> validation,
        Func<IValidation<TField>, IValidation<TReturnedField>> notNullValidation)
        where TField : class
    {
        if (!validation.HasFailed)
        {
            return validation;
        }

        if (validation.GetFieldValue() is not null)
        {
            var validationReturned = notNullValidation(validation.Transform(x => x!));

            if (validationReturned.HasFailed)
            {
                return IValidation<TField?>.CreateFailed();
            }
        }

        return validation;
    }

    public static IValidation<TField?> Null<TField>(
        this IValidation<TField?> validation)
        where TField : class
    {
        return validation.SetValidator(() => new NullValidator<TField?>(validation.GetFieldValue()));
    }

    public static IValidation<TField?> Null<TField>(
        this IValidation<TField?> validation)
        where TField : struct
    {
        return validation.SetValidator(() => new NullValidator<TField?>(validation.GetFieldValue()));
    }

    public static IValidation<TField> GreaterThan<TField>(
        this IValidation<TField> validation,
        TField valueToCompare)
        where TField : IComparisonOperators<TField, TField, bool>
    {
        return validation.SetValidator(() =>
            new GreaterThanValidator<TField>(validation.GetFieldValue(), valueToCompare));
    }

    public static IValidation<TField> Enum<TField>(
        this IValidation<TField> validation)
        where TField : Enum
    {
        return validation.SetValidator(() => new EnumValidator<TField>(validation.GetFieldValue(), typeof(TField)));
    }

    public static IValidation<TEnum> Enum<TEnum>(
        this IValidation<int> validation)
        where TEnum : Enum
    {
        return validation.SetValidator(() => new EnumValidator<int>(validation.GetFieldValue(), typeof(TEnum)))
            .Transform(x => (TEnum)(object)x);
    }

    public static IValidation<TEnum> Enum<TEnum>(
        this IValidation<string> validation)
        where TEnum : Enum
    {
        return validation.SetValidator(() => new EnumValidator<string>(validation.GetFieldValue(), typeof(TEnum)))
            .Transform(x => (TEnum)System.Enum.Parse(typeof(TEnum), x, ignoreCase: true));
    }

    public static IValidation<TField> Enum<TField>(
        this IValidation<TField> validation,
        params TField[] allowedValues)
        where TField : Enum
    {
        return validation.SetValidator(() =>
            new EnumRangeValidator<TField, TField>(validation.GetFieldValue(), allowedValues));
    }

    public static IValidation<TEnum> Enum<TEnum>(
        this IValidation<int> validation,
        params TEnum[] allowedValues)
        where TEnum : Enum
    {
        return validation
            .SetValidator(() => new EnumRangeValidator<int, TEnum>(validation.GetFieldValue(), allowedValues))
            .Transform(x => (TEnum)(object)x);
    }

    public static IValidation<TEnum> Enum<TEnum>(
        this IValidation<string> validation,
        params TEnum[] allowedValues)
        where TEnum : Enum
    {
        return validation.SetValidator(
                () => new EnumRangeValidator<string, TEnum>(validation.GetFieldValue(), allowedValues))
            .Transform(x => (TEnum)System.Enum.Parse(typeof(TEnum), x, ignoreCase: true));
    }

    public static IValidation<string> LengthRange(
        this IValidation<string> validation,
        int minimum,
        int maximum)
    {
        return validation.SetValidator(() => new LengthRangeValidator(validation.GetFieldValue(), minimum, maximum));
    }

    public static IValidation<string> MinimumLength(
        this IValidation<string> validation,
        int minimum)
    {
        return validation.SetValidator(() => new MinimumLengthValidator(validation.GetFieldValue(), minimum));
    }

    public static IValidation<IEnumerable<TInnerType>> ForEach<TInnerType, TReturnedField>(
        this IValidation<IEnumerable<TInnerType>> validation,
        Func<IValidation<TInnerType>, IValidation<TReturnedField>> action)
    {
        if (validation.HasFailed)  // SetValidationScope ???
        {
            return IValidation<IEnumerable<TInnerType>>.CreateFailed();
        }

        validation.IsScopeCreator = true;
        
        var fieldCollection = validation.GetFieldValue()
            .ToList();
        var index = 0;
        var totalItems = fieldCollection.Count();

        foreach (var innerField in fieldCollection)
        {
            var getFieldValue = () => innerField;
            var dependentValidation = CreateDependentValidation(validation, getFieldValue, index);
            action(dependentValidation);
            index++;
            var areAllItemsIterated = (index + 1) == totalItems;
            var stopWithFailedScope =
                validation.HasFailed
                && validation.Context!.ValidationMode is ValidationMode.AccumulateFirstErrors;
            
            if (areAllItemsIterated || stopWithFailedScope)
            {
                validation.HasBeenExecuted = true;
                break;
            }
        }

        return validation.CreateNextValidation();
    }

    public static IValidation<string> Regex(
        this IValidation<string> validation,
        [StringSyntax(StringSyntaxAttribute.Regex)]
        string pattern)
    {
        return validation.SetValidator(() => new RegularExpressionValidator(validation.GetFieldValue(), pattern));
    }
    
    private static IValidation<TDependentField> CreateDependentValidation<TField, TDependentField>(
        IValidation<TField> validation,
        Func<TDependentField> getFieldValue,
        int? index)
    {
        var dependentValidation = new Validation<TDependentField>(
            getFieldValue: getFieldValue,
            crossErrorToException: validation.CrossErrorToException,
            generalizeError: false,
            fieldFullPath: validation.FieldFullPath,
            context: validation.Context,
            index: index,
            parentPath: validation.ParentPath,
            error: null,
            message: null,
            code: null,
            details: null,
            httpStatusCode: null,
            fieldDisplayName: null);
        dependentValidation.HasFailed = validation.HasFailed;
        dependentValidation.HasPendingAsyncValidation = validation.HasPendingAsyncValidation;
        dependentValidation.IsInsideScope = true;
        dependentValidation.ScopeCreatorValidation = validation;
        validation.DependentValidations ??= new();
        validation.DependentValidations.Add(dependentValidation);
        return dependentValidation;
    }
}