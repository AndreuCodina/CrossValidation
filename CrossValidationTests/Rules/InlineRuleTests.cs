using System.Linq;
using CrossValidation;
using CrossValidation.Rules;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Rules;

public class InlineRuleTests
{
    private CreateOrderModel _model;

    public InlineRuleTests()
    {
        _model = new CreateOrderModelBuilder().Build();
    }

    [Fact]
    public void Validator_conditional_execution()
    {
        var expectedErrorMessage = "TrueCase";
        _model = new CreateOrderModelBuilder().Build();
        var action = () => new InlineRule<int>(_model.DeliveryAddress.Number)
            .When(x => false)
            .GreaterThan(_model.DeliveryAddress.Number + 1)
            .When(true)
            .WithMessage(expectedErrorMessage)
            .GreaterThan(_model.DeliveryAddress.Number);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().Message.ShouldBe(expectedErrorMessage);
    }
}