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
        Func<IValidation<TField>, IValidation<TReturnedField>> action)
        where TField : struct
    {
        validation.Condition = () => validation.GetFieldValue() is not null;
        return validation.SetScope(() =>
        {
            validation.IsScopeCreator = true;
            var getFieldValue = () => validation.GetFieldValue()!.Value;
            var scopeValidation = CreateScopeValidation(
                validation: validation,
                getFieldValue: getFieldValue,
                index: validation.Index);
            action(scopeValidation);
        }, ScopeType.WhenNotNull);
    }

    public static IValidation<TField?> WhenNotNull<TField, TReturnedField>(
        this IValidation<TField?> validation,
        Func<IValidation<TField>, IValidation<TReturnedField>> action)
        where TField : class
    {
        validation.Condition = () => validation.GetFieldValue() is not null;
        return validation.SetScope(() =>
        {
            validation.IsScopeCreator = true;
            var getFieldValue = () => validation.GetFieldValue()!;
            var scopeValidation = CreateScopeValidation(
                validation: validation,
                getFieldValue: getFieldValue,
                index: validation.Index);
            action(scopeValidation);
        }, ScopeType.WhenNotNull);
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
        return validation.SetScope(() =>
        {
            validation.IsScopeCreator = true;
            var fieldCollection = validation.GetFieldValue()
                .ToList();
            var index = 0;
            var totalItems = fieldCollection.Count();

            foreach (var innerField in fieldCollection)
            {
                var getFieldValue = () => innerField;
                var scopeValidation = CreateScopeValidation(validation, getFieldValue, index);
                scopeValidation.HasFailed = false;
                action(scopeValidation);
                index++;
                var areAllItemsIterated = index  == totalItems;
                var stopWithFailedScope =
                    validation.HasFailed
                    && validation.Context!.ValidationMode is ValidationMode.AccumulateFirstErrors;
            
                if (areAllItemsIterated || stopWithFailedScope)
                {
                    validation.HasBeenExecuted = true;
                    break;
                }
            }
        }, ScopeType.ForEach);
    }

    public static IValidation<string> Regex(
        this IValidation<string> validation,
        [StringSyntax(StringSyntaxAttribute.Regex)]
        string pattern)
    {
        return validation.SetValidator(() => new RegularExpressionValidator(validation.GetFieldValue(), pattern));
    }

    public static IValidation<TDependentField> CreateScopeValidation<TField, TDependentField>(
        this IValidation<TField> validation,
        Func<TDependentField> getFieldValue,
        int? index)
    {
        var scopeValidation = new Validation<TDependentField>(
            getFieldValue: getFieldValue,
            crossErrorToException: validation.CrossErrorToException,
            generalizeError: validation.GeneralizeError,
            fieldFullPath: validation.FieldFullPath,
            context: validation.Context,
            index: index,
            parentPath: validation.ParentPath,
            fixedError: validation.Context!.Error,
            fixedMessage: validation.Context!.Message,
            fixedCode: validation.Context!.Code,
            fixedDetails: validation.Context!.Details,
            fixedHttpStatusCode: validation.Context!.HttpStatusCode,
            fixedFieldDisplayName: validation.Context!.FieldDisplayName);
        scopeValidation.HasFailed = validation.HasFailed;
        scopeValidation.HasPendingAsyncValidation = validation.HasPendingAsyncValidation;
        scopeValidation.ScopeType = validation.ScopeType;
        scopeValidation.IsInsideScope = true;
        scopeValidation.ScopeCreatorValidation = validation;
        validation.ScopeValidations ??= new();
        validation.ScopeValidations.Add(scopeValidation);
        return scopeValidation;
    }
}