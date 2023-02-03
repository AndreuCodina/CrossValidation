using CrossValidation.Exceptions;
using CrossValidation.Rules;
using CrossValidation.Tests.Builders;
using CrossValidation.Tests.Models;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests;

public class EnsureTests
{
    private ParentModel _model;

    public EnsureTests()
    {
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void Return_EnsureException()
    {
        var action = () => Ensure.That(_model.NullableInt)
            .NotNull();

        action.ShouldThrow<EnsureException>();
    }

    [Fact]
    public void Mark_dsl_as_Ensure()
    {
        var rule = Ensure.That(_model.Int);
        
        rule.ShouldBeAssignableTo<IValidRule<int>>();
        ((IValidRule<int>)rule).Context.Dsl.ShouldBe(Dsl.Ensure);
    }
}