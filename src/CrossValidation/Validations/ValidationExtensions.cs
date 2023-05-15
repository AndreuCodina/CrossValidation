using System.Diagnostics;
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
        if (!validation.HasFailed)
        {
            return validation.SetValidator(() => new NullValidator<TField?>(validation.GetFieldValue()));
        }

        return validation;
    }

    public static IValidation<TField?> Null<TField>(
        this IValidation<TField?> validation)
        where TField : struct
    {
        if (!validation.HasFailed)
        {
            return validation.SetValidator(() => new NullValidator<TField?>(validation.GetFieldValue()));
        }

        return validation;
    }

    public static IValidation<TField> GreaterThan<TField>(
        this IValidation<TField> validation,
        TField valueToCompare)
        where TField : IComparisonOperators<TField, TField, bool>
    {
        if (!validation.HasFailed)
        {
            return validation.SetValidator(() =>
                new GreaterThanValidator<TField>(validation.GetFieldValue(), valueToCompare));
        }

        return validation;
    }

    public static IValidation<TField> Enum<TField>(
        this IValidation<TField> validation)
        where TField : Enum
    {
        if (!validation.HasFailed)
        {
            return validation.SetValidator(() => new EnumValidator<TField>(validation.GetFieldValue(), typeof(TField)));
        }

        return validation;
    }

    public static IValidation<TEnum> Enum<TEnum>(
        this IValidation<int> validation)
        where TEnum : Enum
    {
        var validationToReturn = validation;

        if (!validation.HasFailed)
        {
            validationToReturn =
                validation.SetValidator(() => new EnumValidator<int>(validation.GetFieldValue(), typeof(TEnum)));
        }

        return validationToReturn.Transform(x => (TEnum)(object)x);
    }

    public static IValidation<TEnum> Enum<TEnum>(
        this IValidation<string> validation)
        where TEnum : Enum
    {
        var validationToReturn = validation;

        if (!validation.HasFailed)
        {
            validationToReturn = validation.SetValidator(
                () => new EnumValidator<string>(validation.GetFieldValue(), typeof(TEnum)));
        }

        return validationToReturn.Transform(x =>
            (TEnum)System.Enum.Parse(typeof(TEnum), x, ignoreCase: true));
    }

    public static IValidation<TField> Enum<TField>(
        this IValidation<TField> validation,
        params TField[] allowedValues)
        where TField : Enum
    {
        if (!validation.HasFailed)
        {
            return validation.SetValidator(
                () => new EnumRangeValidator<TField, TField>(validation.GetFieldValue(), allowedValues));
        }

        return validation;
    }

    public static IValidation<TEnum> Enum<TEnum>(
        this IValidation<int> validation,
        params TEnum[] allowedValues)
        where TEnum : Enum
    {
        var validationToReturn = validation;

        if (!validation.HasFailed)
        {
            validationToReturn = validation.SetValidator(
                () => new EnumRangeValidator<int, TEnum>(validation.GetFieldValue(), allowedValues));
        }

        return validationToReturn.Transform(x => (TEnum)(object)x);
    }

    public static IValidation<TEnum> Enum<TEnum>(
        this IValidation<string> validation,
        params TEnum[] allowedValues)
        where TEnum : Enum
    {
        var validationToReturn = validation;

        if (!validation.HasFailed)
        {
            validationToReturn = validation.SetValidator(
                () => new EnumRangeValidator<string, TEnum>(validation.GetFieldValue(), allowedValues));
        }

        return validationToReturn.Transform(x =>
            (TEnum)System.Enum.Parse(typeof(TEnum), x, ignoreCase: true));
    }

    public static IValidation<string> LengthRange(
        this IValidation<string> validation,
        int minimum,
        int maximum)
    {
        if (!validation.HasFailed)
        {
            return validation.SetValidator(() => new LengthRangeValidator(validation.GetFieldValue(), minimum, maximum));
        }

        return validation;
    }

    public static IValidation<string> MinimumLength(
        this IValidation<string> validation,
        int minimum)
    {
        if (!validation.HasFailed)
        {
            return validation.SetValidator(() => new MinimumLengthValidator(validation.GetFieldValue(), minimum));
        }

        return validation;
    }

    public static IValidation<IEnumerable<TReturnedField>> ForEach<TInnerType, TReturnedField>(
        this IValidation<IEnumerable<TInnerType>> validation,
        Func<IValidation<TInnerType>, IValidation<TReturnedField>> action)
    {
        if (validation.HasFailed)
        {
            return IValidation<IEnumerable<TReturnedField>>.CreateFailed();
        }

        throw new NotImplementedException();

        // var fieldCollection = validation.GetFieldValue();
        // var fieldFullPath = validation.Context!.FieldName;
        // var index = 0;
        // var areErrors = false;
        //
        // foreach (var innerField in fieldCollection)
        // {
        //     var newValidation = IValidation<TInnerType>.CreateFromField(
        //         () => innerField,
        //         validation.CrossErrorToException,
        //         generalizeError: validation.Context.GeneralizeError,
        //         fieldFullPath: fieldFullPath,
        //         context: validation.Context,
        //         index: index,
        //         parentPath: validation.Context.ParentPath,
        //         error: validation.Context.Error,
        //         message: validation.Context.Message,
        //         code: validation.Context.Code,
        //         details: validation.Context.Details,
        //         httpStatusCode: validation.Context.HttpStatusCode,
        //         fieldDisplayName: validation.Context.FieldDisplayName);
        //     var validationReturned = action(newValidation);
        //
        //     if (validationReturned.HasFailed)
        //     {
        //         if (validation.Context.ValidationMode is ValidationMode.StopOnFirstError
        //             || validation.Context.ValidationMode is ValidationMode.AccumulateFirstError)
        //         {
        //             return IValidation<IEnumerable<TReturnedField>>.CreateFailed();
        //         }
        //         else if (validation.Context.ValidationMode is ValidationMode
        //                      .AccumulateFirstErrorAndAllFirstErrorsCollectionIteration)
        //         {
        //             areErrors = true;
        //             index++;
        //             continue;
        //         }
        //         else
        //         {
        //             throw new UnreachableException();
        //         }
        //     }
        //
        //     index++;
        // }
        //
        // if (areErrors)
        // {
        //     return IValidation<IEnumerable<TReturnedField>>.CreateFailed();
        // }
        // else
        // {
        //     return IValidation<IEnumerable<TReturnedField>>.CreateFromField(
        //         () => throw new InvalidOperationException("Cannot continue a validation after ForEah"),
        //         validation.CrossErrorToException,
        //         fieldFullPath: fieldFullPath,
        //         context: validation.Context,
        //         index: index,
        //         parentPath: validation.Context.ParentPath);
        // }
    }

    public static IValidation<string> Regex(
        this IValidation<string> validation,
        [StringSyntax(StringSyntaxAttribute.Regex)]
        string pattern)
    {
        if (!validation.HasFailed)
        {
            return validation.SetValidator(
                () => new RegularExpressionValidator(validation.GetFieldValue(), pattern));
        }

        return validation;
    }
}