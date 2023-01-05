using CrossValidation;
using CrossValidation.Extensions;
using CrossValidation.Rules;
using CrossValidationTests.Builders;
using CrossValidationTests.Fixtures;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Rules;

public class RuleExtensionTests_NotNull : IClassFixture<Fixture>
{
    private readonly Fixture _fixture;
    private ParentModel _model;

    public RuleExtensionTests_NotNull(Fixture fixture)
    {
        _fixture = fixture;
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
            .Must(_fixture.BeValid);
        action.ShouldNotThrow();
    }
    
        
    [Fact]
    public void NotNull_works_with_nullable_types_and_error_accumulation()
    {
        _model = new ParentModelBuilder()
            .WithNullableString("Value")
            .Build();
        var parentModelValidator = _fixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachRule;

            validator.RuleFor(x => x.NullableInt)
                .NotNull()
                .GreaterThan(-1);

            validator.RuleFor(x => x.NullableString)
                .NotNull()
                .Length(int.MaxValue, int.MaxValue);

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
}