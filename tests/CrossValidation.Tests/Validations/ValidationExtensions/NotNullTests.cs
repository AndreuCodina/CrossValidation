﻿using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Fixtures;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

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
            validator.ValidationMode = ValidationMode.AccumulateFirstErrors;

            validator.Field(_model.NullableInt)
                .NotNull()
                .GreaterThan(-1);

            validator.Field(_model.NullableString)
                .NotNull()
                .LengthRange(int.MaxValue, int.MaxValue);

            validator.Field(_model.NullableInt)
                .NotNull();
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var errors = action.ShouldThrowCrossErrors();
        errors.Count.ShouldBe(3);
        errors[0].ShouldBeOfType<CommonCrossError.NotNull>();
        errors[1].ShouldBeOfType<CommonCrossError.LengthRange>();
        errors[2].ShouldBeOfType<CommonCrossError.NotNull>();
    }
    
    [Fact]
    public void Return_error_when_the_validation_fails()
    {
        var action = () => Validate.Field(_model.NullableString)
            .NotNull();

        var error = action.ShouldThrowCrossError<CommonCrossError.NotNull>();
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
    }
}