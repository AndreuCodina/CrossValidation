using CrossValidation.Errors;
using CrossValidation.Rules;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.Builders;
using CrossValidation.Tests.Fixtures;
using CrossValidation.Tests.Models;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Rules.RuleExtensions;

public class WhenNotNullTests : IClassFixture<CommonFixture>
{
    private readonly CommonFixture _commonFixture;
    private ParentModel _model;

    public WhenNotNullTests(CommonFixture commonFixture)
    {
        _commonFixture = commonFixture;
        _model = new ParentModelBuilder().Build();
    }

    [Fact]
    public void WhenNotNull_does_not_fail_when_field_is_null()
    {
        var structRuleAction = () => Validate.That(_model.NullableInt)
            .WhenNotNull(x => x
                .Must(_commonFixture.NotBeValid));
        structRuleAction.ShouldNotThrow();

        var classRuleAction = () => Validate.That(_model.NullableString)
            .WhenNotNull(x => x
                .Must(_commonFixture.NotBeValid));
        classRuleAction.ShouldNotThrow();
    }

    [Fact]
    public void WhenNotNull_fails_with_transformed_field_value()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .WithNullableString("Value")
            .Build();
        
        var structRuleAction = () => Validate.That(_model.NullableInt)
            .WhenNotNull(x => x
                .GreaterThan(_model.NullableInt!.Value));
        var error = structRuleAction.ShouldThrowValidationError();
        error.FieldValue.ShouldNotBeOfType<int?>();
        error.FieldValue.ShouldBeOfType<int>();

        var classRuleAction = () => Validate.That(_model.NullableString)
            .WhenNotNull(x => x
                .Must(_commonFixture.NotBeValid));
        error = classRuleAction.ShouldThrowValidationError();
        error.FieldValue.ShouldBeOfType<string>();
    }
    
    [Fact]
    public void Return_invalid_rule_when_inner_validation_fail()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .WithNullableString("Value")
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachRule;
            
            var structRuleAction = validator.RuleFor(x => x.NullableInt)
                .WhenNotNull(x => x
                    .Must(_commonFixture.NotBeValid));
            (structRuleAction is IInvalidRule<int?>).ShouldBeTrue();
            
            var classRuleAction = validator.RuleFor(x => x.NullableString)
                .WhenNotNull(x => x
                    .Must(_commonFixture.NotBeValid));
            (classRuleAction is IInvalidRule<string>).ShouldBeTrue();
        });

        var action = () => parentModelValidator.Validate(_model);

        action.ShouldThrowValidationErrors();
    }

    [Fact]
    public void Inner_rules_can_return_a_different_type()
    {
        var structRuleAction = () => Validate.That(_model.NullableInt)
            .WhenNotNull(x => x
                .Transform(x => x.ToString()));
        structRuleAction.ShouldNotThrow();
        
        var classRuleAction = () => Validate.That(_model.NullableString)
            .WhenNotNull(x => x
                .Transform(x => x.ToString()));
        classRuleAction.ShouldNotThrow();
    }


    [Fact]
    public void Keep_type_after_rule_nested_transformed()
    {
        var action = () => Validate.That(_model.NullableInt)
            .WhenNotNull(x => x
                .Must(_commonFixture.BeValid))
            .NotNull();

        var error = action.ShouldThrowValidationError<CommonValidationError.NotNull>();
        error.FieldValue.ShouldNotBeOfType<int>();
        error.FieldValue.ShouldBeNull();
    }
}