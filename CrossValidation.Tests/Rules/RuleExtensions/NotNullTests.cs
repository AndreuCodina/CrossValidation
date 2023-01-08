using CrossValidation.Resources;
using CrossValidation.Rules;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.Builders;
using CrossValidation.Tests.Fixtures;
using CrossValidation.Tests.Models;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Rules.RuleExtensions;

public class NotNullTests : IClassFixture<CommonFixture>
{
    private readonly CommonFixture _commonFixture;
    private ParentModel _model;

    public NotNullTests(CommonFixture commonFixture)
    {
        _commonFixture = commonFixture;
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void NotNull_works_with_nullable_value_types()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();

        var action = () => Validate.That(_model.NullableInt)
            .NotNull()
            .GreaterThan(_model.NullableInt!.Value - 1);
        action.ShouldNotThrow();
    }
    
    [Fact]
    public void NotNull_works_with_nullable_reference_types()
    {
        _model = new ParentModelBuilder()
            .WithNullableString("The string")
            .Build();

        var action = () => Validate.That(_model.NullableString)
            .NotNull()
            .Must(_commonFixture.BeValid);
        action.ShouldNotThrow();
    }
    
        
    [Fact]
    public void NotNull_works_with_nullable_types_and_error_accumulation()
    {
        _model = new ParentModelBuilder()
            .WithNullableString("Value")
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachRule;

            validator.RuleFor(x => x.NullableInt)
                .NotNull()
                .GreaterThan(-1);

            validator.RuleFor(x => x.NullableString)
                .NotNull()
                .LengthRange(int.MaxValue, int.MaxValue);

            validator.RuleFor(x => x.NullableInt)
                .NotNull();
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var errors = action.ShouldThrowValidationErrors();
        errors.Count.ShouldBe(3);
        errors[0].ShouldBeOfType<CommonCrossValidationError.NotNull>();
        errors[1].ShouldBeOfType<CommonCrossValidationError.LengthRange>();
        errors[2].ShouldBeOfType<CommonCrossValidationError.NotNull>();
    }
    
    [Fact]
    public void Return_error_when_the_validation_fails()
    {
        string? value = null;

        var action = () => Validate.That(value)
            .NotNull();

        var error = action.ShouldThrowValidationError<CommonCrossValidationError.NotNull>();
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
    }
}