using CrossValidation;
using CrossValidation.Rules;
using CrossValidationTests.Builders;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Rules;

public class InlineRuleTests
{
    private readonly ParentModel _model;

    public InlineRuleTests()
    {
        _model = new ParentModelBuilder().Build();
    }

    [Fact]
    public void Validator_conditional_execution()
    {
        var expectedMessage = "TrueCase";
        var action = () => new InlineRule<int>(_model.NestedModel.Int)
            .When(x => false)
            .GreaterThan(_model.NestedModel.Int + 1)
            .When(true)
            .WithMessage(expectedMessage)
            .GreaterThan(_model.NestedModel.Int);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.Message.ShouldBe(expectedMessage);
    }
}