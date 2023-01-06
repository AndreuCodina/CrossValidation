using CrossValidation;
using CrossValidation.Extensions;
using CrossValidation.Rules;
using CrossValidationTests.Builders;
using CrossValidationTests.Fixtures;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Rules.RuleExtensions;

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
    public void WhenNotNull_does_not_fail_when_the_field_is_null()
    {
        var action = () => Validate.That(_model.NullableInt)
            .WhenNotNull(x => x
                .Must(_commonFixture.NotBeValid));

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

        var error = action.ShouldThrowValidationError();
        error.ShouldNotBeOfType<CommonCrossValidationError.GreaterThan<int?>>();
        error.ShouldBeOfType<CommonCrossValidationError.GreaterThan<int>>();
    }

    [Fact]
    public void WhenNotNull_fails_with_transformed_value_from_nullable_reference_type()
    {
        _model = new ParentModelBuilder()
            .WithNullableString("Value")
            .Build();

        var action = () => Validate.That(_model.NullableString)
            .WhenNotNull(x => x
                .Must(_commonFixture.NotBeValid));

        var error = action.ShouldThrowValidationError();
        error.FieldValue.ShouldBeOfType<string?>();
        error.FieldValue.ShouldBeOfType<string?>();
    }

    [Fact]
    public void Keep_type_after_rule_nested_transformed()
    {
        var action = () => Validate.That(_model.NullableInt)
            .WhenNotNull(x => x
                .Must(_commonFixture.BeValid))
            .NotNull();

        var error = action.ShouldThrowValidationError<CommonCrossValidationError.NotNull>();
        error.FieldValue.ShouldNotBeOfType<int>();
        error.FieldValue.ShouldBeNull();
    }
}