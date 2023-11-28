using Common.Tests;
using CrossValidation.ErrorResources;
using CrossValidation.Exceptions;
using CrossValidation.UnitTests.TestUtils.Builders;
using CrossValidation.UnitTests.TestUtils.Fixtures;
using CrossValidation.UnitTests.TestUtils.Models;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

public class NotNullTests :
    TestBase,
    IClassFixture<CommonFixture>
{
    private readonly CommonFixture _commonFixture;
    private ParentModel _model;

    public NotNullTests(CommonFixture commonFixture)
    {
        _commonFixture = commonFixture;
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void Validate_nullable_value_types()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();

        var action = () => Validate.That(_model.NullableInt)
            .NotNull()
            .GreaterThan(_model.NullableInt!.Value - 1);
        
        action.Should()
            .NotThrow();
    }
    
    [Fact]
    public void Validate_nullable_reference_types()
    {
        _model = new ParentModelBuilder()
            .WithNullableString("The string")
            .Build();

        var action = () => Validate.That(_model.NullableString)
            .NotNull()
            .Must(_commonFixture.BeValid);
        
        action.Should()
            .NotThrow();
    }
    
        
    [Fact]
    public void Validate_nullable_types_using_error_accumulation()
    {
        _model = new ParentModelBuilder()
            .WithNullableString("Value")
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorRelatedToField;

            validator.Field(_model.NullableInt)
                .NotNull()
                .GreaterThan(-1);

            validator.Field(_model.NullableString)
                .NotNull()
                .InclusiveLengthRange(int.MaxValue, int.MaxValue);

            validator.Field(_model.NullableInt)
                .NotNull();
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var exceptions = action.Should()
            .Throw<BusinessListException>()
            .Which
            .Exceptions;
        exceptions.Should()
            .HaveCount(3);
        exceptions[0].Should()
            .BeOfType<CommonException.NotNullException>();
        exceptions[1].Should()
            .BeOfType<CommonException.InclusiveLengthRangeException>();
        exceptions[2].Should()
            .BeOfType<CommonException.NotNullException>();
    }
    
    [Fact]
    public void Throw_exception_when_the_validation_fails()
    {
        var action = () => Validate.Field(_model.NullableString)
            .NotNull();

        action.Should()
            .Throw<CommonException.NotNullException>()
            .And
            .Code
            .Should()
            .Be(nameof(ErrorResource.NotNull));
    }
}