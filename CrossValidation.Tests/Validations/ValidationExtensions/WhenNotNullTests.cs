using CrossValidation.Errors;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.Builders;
using CrossValidation.Tests.Fixtures;
using CrossValidation.Tests.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

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
        var structValidationAction = () => Validate.That(_model.NullableInt)
            .WhenNotNull(x => x
                .Must(_commonFixture.NotBeValid));
        structValidationAction.ShouldNotThrow();

        var classValidationAction = () => Validate.That(_model.NullableString)
            .WhenNotNull(x => x
                .Must(_commonFixture.NotBeValid));
        classValidationAction.ShouldNotThrow();
    }

    [Fact]
    public void WhenNotNull_fails_with_transformed_field_value()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .WithNullableString("Value")
            .Build();
        
        var structValidationAction = () => Validate.That(_model.NullableInt)
            .WhenNotNull(x => x
                .GreaterThan(_model.NullableInt!.Value));
        var error = structValidationAction.ShouldThrowValidationError();
        error.FieldValue.ShouldNotBeOfType<int?>();
        error.FieldValue.ShouldBeOfType<int>();

        var classValidationAction = () => Validate.That(_model.NullableString)
            .WhenNotNull(x => x
                .Must(_commonFixture.NotBeValid));
        error = classValidationAction.ShouldThrowValidationError();
        error.FieldValue.ShouldBeOfType<string>();
    }
    
    [Fact]
    public void Return_invalid_validation_when_inner_validation_fail()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .WithNullableString("Value")
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachValidation;
            
            var structValidationAction = validator.Field(_model.NullableInt)
                .WhenNotNull(x => x
                    .Must(_commonFixture.NotBeValid));
            (structValidationAction is IInvalidValidation<int?>).ShouldBeTrue();
            
            var classValidationAction = validator.Field(_model.NullableString)
                .WhenNotNull(x => x
                    .Must(_commonFixture.NotBeValid));
            (classValidationAction is IInvalidValidation<string>).ShouldBeTrue();
        });

        var action = () => parentModelValidator.Validate(_model);

        action.ShouldThrowValidationErrors();
    }

    [Fact]
    public void Inner_validations_can_return_a_different_type()
    {
        var structValidationAction = () => Validate.That(_model.NullableInt)
            .WhenNotNull(x => x
                .Transform(x => x.ToString()));
        structValidationAction.ShouldNotThrow();
        
        var classValidationAction = () => Validate.That(_model.NullableString)
            .WhenNotNull(x => x
                .Transform(x => x.ToString()));
        classValidationAction.ShouldNotThrow();
    }


    [Fact]
    public void Keep_type_after_validation_nested_transformed()
    {
        var action = () => Validate.That(_model.NullableInt)
            .WhenNotNull(x => x
                .Must(_commonFixture.BeValid))
            .NotNull();

        var error = action.ShouldThrowValidationError<CommonCrossError.NotNull>();
        error.FieldValue.ShouldNotBeOfType<int>();
        error.FieldValue.ShouldBeNull();
    }
}