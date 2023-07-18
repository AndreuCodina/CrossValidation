using System.Diagnostics.Contracts;
using System.Net;
using System.Runtime.CompilerServices;
using CrossValidation.Exceptions;
using CrossValidation.Validations;

namespace CrossValidation;

public abstract record ModelValidator<TModel>
{
    internal ValidationMode _validationMode = ValidationMode.StopOnFirstError;
    internal IValidation<TModel>? ScopeCreatorValidation { private get; set; }
    public TModel Model { get; set; } = default!;

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
        BusinessException? exception = null,
        string message = "",
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null,
        string? fieldDisplayName = null,
        [CallerArgumentExpression(nameof(field))] string fieldName = default!)
    {
        var fieldPath = fieldName.Contains('.')
            ? fieldName.Substring(fieldName.IndexOf('.') + 1)
            : fieldName;
        ScopeCreatorValidation!.FieldPath = fieldPath;
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
        BusinessException? exception = null,
        string message = "",
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

    public abstract void CreateValidations();

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
        Model = model;
        ScopeCreatorValidation = CrossValidation.Validate
                .That(model);
        ScopeCreatorValidation.IsScopeCreator = true;
        ScopeCreatorValidation.SetScope(CreateValidations, ScopeType.ModelValidator);

        if (useAsync)
        {
            await ScopeCreatorValidation.ValidateAsync();
        }
        
        if (ScopeCreatorValidation.Context!.ExceptionsCollected.Any())
        {
            if (ScopeCreatorValidation.Context.ExceptionsCollected.Count == 1)
            {
                throw ScopeCreatorValidation.Context
                    .ExceptionsCollected[0];
            }
            else
            {
                foreach (var errorCollected in ScopeCreatorValidation.Context.ExceptionsCollected)
                {
                    errorCollected.AddCommonPlaceholderValues();
                }
                
                throw new ValidationListException(ScopeCreatorValidation.Context.ExceptionsCollected);
            }
        }
    }
}