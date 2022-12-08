using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using CrossValidation.Results;
using CrossValidation.Rules;
using CrossValidation.ValidationContexts;

namespace CrossValidation;
// Foo
// Test2
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
        var context = Context!;
        // context.FieldName = "";
        // context.FieldValue = null;
        return new ModelRule<TModel, TField>(Model, fieldSelector, context);
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

        // If it's not a child model and the validation mode is
        if (!Context.IsChildContext
            && Context.Errors is not null
            && Context.Errors.Any()) // TODO: Remove second condition?
        {
            throw new ValidationException(Context.Errors!);
        }
    }

    // public List<ValidationError> ValidateAndReturnErrors(TModel model, ModelValidationContext childContext)
    // {
    //     Model = (TModel)model;
    //     Context = childContext;
    //     Context.ExecutionMode = ExecutionMode.ReturnErrors;
    //     var errors = new List<ValidationError>();
    //     CreateRules();
    //     
    //     // foo:
    //     //     Console.WriteLine();
    //
    //     if (Context.Errors is not null)
    //     {
    //         errors.AddRange(Context.Errors);
    //     }
    //
    //     return errors;
    // }

    // public void SetContext(ModelValidationContext childContext)
    // {
    //     Context = childContext;
    // }
}