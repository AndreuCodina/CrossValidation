using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using CrossValidation.Exceptions;
using CrossValidation.Rules;
using CrossValidation.ValidationContexts;

namespace CrossValidation;

public abstract record ModelValidator<TModel>
{
    private ValidationMode _validationMode = ValidationMode.StopValidationOnFirstError;
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
    public IRule<TField> Field<TField>(
        TField field,
        [CallerArgumentExpression(nameof(field))]
        string fieldName = "")
    {
        return IValidRule<TField>.CreateFromFieldName(field, fieldName, Context);
    }

    [Pure]
    public IRule<TField> That<TField>(TField fieldValue)
    {
        return IValidRule<TField>.CreateFromField(fieldValue, context: Context);
    }

    public abstract void CreateRules(TModel model);

    public void Validate(TModel model)
    {
        if (Context is not {IsChildContext: true})
        {
            CrossValidation.Validate.ModelNullability(model);
            Context = new ValidationContext();
        }

        Model = model;
        CreateRules(model);

        if (!Context.IsChildContext && Context.ErrorsCollected is not null)
        {
            if (Context.ErrorsCollected.Count == 1)
            {
                throw new CrossException(Context.ErrorsCollected[0]);
            }
            else
            {
                throw new ValidationListException(Context.ErrorsCollected);
            }
        }
    }
}