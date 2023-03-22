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
        Context!.ExecuteAccumulatedOperations<TField>();
        return IValidValidation<TField>.CreateFromFieldName(
            () => field,
            typeof(CrossException),
            fieldName,
            allowFieldNameWithoutModel: false,
            context: Context,
            error: error,
            message: message,
            code: code,
            details: details,
            httpStatusCode: httpStatusCode,
            fieldDisplayName: fieldDisplayName);
    }

    [Pure]
    public IValidation<TField> That<TField>(
        TField fieldValue,
        ICrossError? error = null,
        string? message = null,
        string? code = null,
        string? details = null,
        HttpStatusCode? httpStatusCode = null,
        string? fieldDisplayName = null)
    {
        Context!.ExecuteAccumulatedOperations<TField>();
        return IValidValidation<TField>.CreateFromField(
            () => fieldValue,
            typeof(CrossException),
            context: Context,
            error: error,
            message: message,
            code: code,
            details: details,
            httpStatusCode: httpStatusCode,
            fieldDisplayName: fieldDisplayName);
    }

    public abstract void CreateValidations(TModel model);

    public void Validate(TModel model)
    {
        if (Context is not {IsChildContext: true})
        {
            CrossValidation.Validate.ModelNullability(model);
            Context = new ValidationContext();
        }

        Model = model;
        CreateValidations(model);
        Context.ExecuteAccumulatedOperations<object>(); // Pass any type because we aren't going to return a validation

        if (!Context.IsChildContext && Context.ErrorsCollected is not null)
        {
            if (Context.ErrorsCollected.Count == 1)
            {
                throw Context.ErrorsCollected[0].ToException();
            }
            else
            {
                throw new ValidationListException(Context.ErrorsCollected);
            }
        }
    }
}