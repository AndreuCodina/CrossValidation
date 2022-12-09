using System.Linq.Expressions;
using CrossValidation.Rules;
using CrossValidation.ValidationContexts;

namespace CrossValidation;
// TODO: Convert to a class to run the tests
// Temporary solution until https://github.com/castleproject/Core/issues/632 is fixed
public abstract class ModelValidator<TModel>
{
    private ValidationMode _validationMode = ValidationMode.StopOnFirstError;
    public ModelValidationContext? Context { get; set; } = null;
    public TModel Model { get; set; }

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

    public ModelRule<TModel, TField> RuleFor<TField>(Expression<Func<TModel, TField>> fieldSelector)
    {
        return ModelRule<TModel, TField>.Create(Model, Context!, fieldSelector);
    }

    public abstract void CreateRules();

    public void Validate(TModel model)
    {
        Model = model;

        if (Context is not {IsChildContext: true})
        {
            Context = new ModelValidationContext();
        }

        CreateRules();

        if (!Context.IsChildContext && Context.Errors is not null)
        {
            throw new ValidationException(Context.Errors!);
        }
    }
}