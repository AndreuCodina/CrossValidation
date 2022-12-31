using System.Linq.Expressions;
using CrossValidation.Exceptions;
using CrossValidation.Rules;
using CrossValidation.Utils;
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
            ModelNullabilityValidator.Validate(model);
            Context = new ValidationContext();
        }

        Model = model;
        CreateRules(model);

        if (!Context.IsChildContext && Context.Errors is not null)
        {
            throw new CrossValidationException(Context.Errors!);
        }
    }
}