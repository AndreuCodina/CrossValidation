namespace CrossValidation;

public abstract class Validate
{
    // public static void Model<TModel>(TModel model, Action<ContextInitializer<TModel>> rule)
    // {
    //     var ruleBuilder = new ContextInitializer<TModel>(model);
    //     rule(ruleBuilder);
    // }

    // public static InlineRuleBuilder<TProperty> That<TProperty>(TProperty property)
    // {
    //     return new InlineRuleBuilder<TProperty>(property);
    // }
    
    // TProperty Return(TProperty property)
    // TResult Return/Function(Func<TProperty, TResult> f) -> Validate.Return(ProductName.Create(request.Name))
    // -> var name = Validate.That(request.Name).Return(ProductName.Create); // or .Construct(ProductName) // with new()
    // or
    // var name = Validate.Return<string>(ProductName(request.Name));
    // or
    // var name = Validate.(And)Return(request.Name, ProductName.Create);
    
    // "Alias" for InlineRuleBuilder<bool> and you can call
    // Ensure.Is(orderLines.Length == 0)
    //     .WithError(...)
    //     .IsTrue()
    // Or better: Do that inside the helper function:
    // void Is/Ensure/ValidateWithError/Message(bool condition, TError error) { ... }
    // public static InlineRuleBuilder<TException> Is(bool condition, new MyApp/DomainException)
    // {
    //     return new InlineRuleBuilder<TProperty>(property);
    // }
    
    // Validate.ThrowIf<DomainException>(request.Name.Length > 0)
    // Validate.ThrowIf<DomainException>(request.Name.Length, x => x.GreaterThan(0), "The optional error message")
    
    // public static void Model<TModel, TProperty>(TModel model, Expression<Func<TModel, TProperty>> selector)
    // {
    //     var expression = (MemberExpression)selector.Body;
    //     var name = expression.Member.Name;
    //     return;
    // }

    // public static void That(bool condition)
    // {
    //     
    // }
    
    // public static void That<T>(T model)
    // {
    //     return;
    // }
}