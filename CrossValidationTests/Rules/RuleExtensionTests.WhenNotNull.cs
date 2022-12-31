using CrossValidation;
using CrossValidation.Exceptions;
using CrossValidation.Rules;
using CrossValidationTests.Builders;
using CrossValidationTests.Fixtures;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Rules;

public class RuleExtensionTests_WhenNotNull : IClassFixture<Fixture>
{
    private readonly Fixture _fixture;
    private ParentModel _model;

    public RuleExtensionTests_WhenNotNull(Fixture fixture)
    {
        _fixture = fixture;
        _model = new ParentModelBuilder().Build();
    }

    [Fact]
    public void WhenNotNull_does_not_fail_when_the_field_is_null()
    {
        var action = () => Validate.That(_model.NullableInt)
            .WhenNotNull(x => x
                .Must(_fixture.NotBeValid));

        action.ShouldNotThrow();
    }

    [Fact]
    public void WhenNotNull_fails_with_transformed_value_from_nullable_value_type()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();

        var action = () => Validate.That(_model.NullableInt)
            .WhenNotNull(x => x
                .GreaterThan(_model.NullableInt!.Value));

        var errors = action.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBe(1);
        errors[0].ShouldNotBeOfType<CommonCrossValidationError.GreaterThan<int?>>();
        errors[0].ShouldBeOfType<CommonCrossValidationError.GreaterThan<int>>();
    }

    [Fact]
    public void WhenNotNull_fails_with_transformed_value_from_nullable_reference_type()
    {
        _model = new ParentModelBuilder()
            .WithNullableString("Value")
            .Build();

        var action = () => Validate.That(_model.NullableString)
            .WhenNotNull(x => x
                .Must(_fixture.NotBeValid));

        var errors = action.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBe(1);
        errors[0].FieldValue.ShouldBeOfType<string?>();
        errors[0].FieldValue.ShouldBeOfType<string?>();
    }

    [Fact]
    public void Keep_type_after_rule_nested_transformed()
    {
        var action = () => Validate.That(_model.NullableInt)
            .WhenNotNull(x => x
                .Must(_fixture.BeValid))
            .NotNull();

        var errors = action.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBe(1);
        errors[0].ShouldBeOfType<CommonCrossValidationError.NotNull>();
        errors[0].FieldValue.ShouldNotBeOfType<int>();
        errors[0].FieldValue.ShouldBeNull();
    }
}