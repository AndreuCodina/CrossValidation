using System.Linq.Expressions;
using CrossValidation.Rules;
using CrossValidation.Utils;
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

    public void RuleForEach<TInnerType>(
        Expression<Func<TModel, IEnumerable<TInnerType>>> collectionSelector,
        Action<ModelRule<TModel, TInnerType>> action)
    {
        var collectionSelected = collectionSelector.Compile()(Model);
        var fieldInformation = Util.GetFieldInformation(collectionSelector, Model);
        var index = 0;

        foreach (var innerField in collectionSelected)
        {
            Expression<Func<TModel, TInnerType>> innerFieldSelector = model => innerField;
            var rule = new ModelRule<TModel, TInnerType>(
                Context,
                Model,
                fieldInformation.SelectionFullPath,
                innerField,
                fieldInformation.IsFieldSelectedDifferentThanModel,
                innerFieldSelector,
                index);
            action(rule);
            index++;
        }
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
            throw new CrossValidationException(Context.Errors!);
        }
    }
}