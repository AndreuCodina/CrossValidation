using System;
using System.Threading.Tasks;
using CrossValidation.Errors;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Fixtures;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

public class WhenNotNullTests :
    TestBase,
    IClassFixture<CommonFixture>
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
        var error = structValidationAction.ShouldThrowCrossError();
        error.GetFieldValue!().ShouldNotBeOfType<int?>();
        error.GetFieldValue!().ShouldBeOfType<int>();

        var classValidationAction = () => Validate.That(_model.NullableString)
            .WhenNotNull(x => x
                .Must(_commonFixture.NotBeValid));
        error = classValidationAction.ShouldThrowCrossError();
        error.GetFieldValue!().ShouldBeOfType<string>();
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
            validator.ValidationMode = ValidationMode.AccumulateFirstErrors;
            
            var structValidationAction = validator.Field(_model.NullableInt)
                .WhenNotNull(x => x
                    .Must(_commonFixture.NotBeValid));
            structValidationAction.HasFailed.ShouldBeTrue();
            
            var classValidationAction = validator.Field(_model.NullableString)
                .WhenNotNull(x => x
                    .Must(_commonFixture.NotBeValid));
            classValidationAction.HasFailed.ShouldBeTrue();
        });

        var action = () => parentModelValidator.Validate(_model);

        action.ShouldThrowCrossErrors();
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

        var error = action.ShouldThrowCrossError<CommonCrossError.NotNull>();
        error.GetFieldValue!().ShouldNotBeOfType<int>();
        error.GetFieldValue!().ShouldBeNull();
    }
    
    [Fact]
    public void Accumulate_errors_with_error_accumulation()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();
         
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrors;

            validator.Field(_model.NullableInt)
                .WhenNotNull(x => x
                    .Must(_commonFixture.NotBeValid));
            
            validator.Field(_model.Int)
                .Must(_commonFixture.NotBeValid);
        });

        var action = () => parentModelValidator.Validate(_model);

        var errors = action.ShouldThrowCrossErrors();
        errors.Count.ShouldBe(2);
    }
    
    [Fact]
    public async Task Accumulate_errors_with_operation_accumulation()
    {
        var expectedMessage = "Expected message";
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();

        IValidation<int> WhenNotNullFunc()
        {
            throw new Exception();
        };

        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrors;

            validator.Field(_model.NullableInt)
                .WithMessage(expectedMessage)
                .MustAsync(_commonFixture.NotBeValidAsync)
                .WhenNotNull(_ => WhenNotNullFunc())
                .Transform(x => (int?)null)
                .Must(x => x is null);

            validator.Field(_model.Int)
                .Must(_commonFixture.NotBeValid);
        });
        var action = () => parentModelValidator.ValidateAsync(_model);

        var errors = await action.ShouldThrowCrossErrorsAsync();
        errors.Count.ShouldBe(2);
        errors[0].ShouldBeOfType<CommonCrossError.Predicate>();
        errors[0].Message.ShouldBe(expectedMessage);
    }
}