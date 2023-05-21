using System.Diagnostics.Contracts;
using System.Net;
using System.Runtime.CompilerServices;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Validations;

namespace CrossValidation;

public abstract record ModelValidator<TModel>
{
    private ValidationMode _validationMode = ValidationMode.StopOnFirstError;
    internal IValidation<TModel>? ScopeCreatorValidation { private get; set; }

    public ValidationMode ValidationMode
    {
        get => _validationMode;
        set
        {
            if (ScopeCreatorValidation!.Context!.IsChildContext)
            {
                throw new InvalidOperationException("Cannot change the validation mode in a child model validator");
            }

            _validationMode = value;
            ScopeCreatorValidation.Context!.ValidationMode = _validationMode;
        }
    }

    [Pure]
    public IValidation<TField> Field<TField>(
        TField field,
        ICrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null,
        string? fieldDisplayName = null,
        [CallerArgumentExpression(nameof(field))] string fieldName = default!)
    {
        var fieldPath = fieldName.Contains('.')
            ? fieldName.Substring(fieldName.IndexOf('.') + 1)
            : fieldName;
        ScopeCreatorValidation!.FieldPath = fieldPath; // TODO: Remove ??
        var getFieldValue = () => field;
        var scopeValidation = ScopeCreatorValidation.CreateScopeValidation(
            getFieldValue: getFieldValue,
            index: null,
            fieldPathToOverride: null);
        scopeValidation.HasFailed = false;
        scopeValidation.GeneralizeError = false;
        return scopeValidation;
    }

    [Pure]
    public IValidation<TField> That<TField>(
        TField field,
        ICrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null,
        string? fieldDisplayName = null)
    {
        var getFieldValue = () => field;
        var scopeValidation = ScopeCreatorValidation!.CreateScopeValidation(
            getFieldValue: getFieldValue,
            index: null,
            fieldPathToOverride: null);
        scopeValidation.HasFailed = false;
        scopeValidation.GeneralizeError = true;
        return scopeValidation;
    }

    public abstract void CreateValidations(TModel model);

    public void Validate(TModel model)
    {
        InternalValidateAsync(model, useAsync: false)
            .GetAwaiter()
            .GetResult();
    }
    
    public async Task ValidateAsync(TModel model)
    {
        await InternalValidateAsync(model, useAsync: true);
    }

    private async ValueTask InternalValidateAsync(TModel model, bool useAsync)
    {
        CrossValidation.Validate.ModelNullability(model);
        ScopeCreatorValidation = CrossValidation.Validate
                .That(model);
        ScopeCreatorValidation.IsScopeCreator = true;
        ScopeCreatorValidation.SetScope(() => CreateValidations(model), ScopeType.ModelValidator);

        if (useAsync)
        {
            await ScopeCreatorValidation.ValidateAsync();
        }
        
        if (ScopeCreatorValidation.Context!.ErrorsCollected.Any())
        {
            if (ScopeCreatorValidation.Context.ErrorsCollected.Count == 1)
            {
                throw ScopeCreatorValidation.Context.ErrorsCollected[0].ToException();
            }
            else
            {
                foreach (var errorCollected in ScopeCreatorValidation.Context.ErrorsCollected)
                {
                    errorCollected.AddPlaceholderValues();
                }
                throw new ValidationListException(ScopeCreatorValidation.Context.ErrorsCollected);
            }
        }
    }
}