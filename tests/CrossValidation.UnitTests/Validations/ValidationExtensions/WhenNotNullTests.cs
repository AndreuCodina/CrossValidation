using Common.Tests;
using CrossValidation.Exceptions;
using CrossValidation.UnitTests.TestUtils.Builders;
using CrossValidation.UnitTests.TestUtils.Fixtures;
using CrossValidation.UnitTests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

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
        var exception = structValidationAction.ShouldThrow<BusinessException>();
        exception.GetFieldValue!().ShouldNotBeOfType<int?>();
        exception.GetFieldValue!().ShouldBeOfType<int>();

        var classValidationAction = () => Validate.That(_model.NullableString)
            .WhenNotNull(x => x
                .Must(_commonFixture.NotBeValid));
        exception = classValidationAction.ShouldThrow<BusinessException>();
        exception.GetFieldValue!().ShouldBeOfType<string>();
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
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorRelatedToField;
            
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

        action.ShouldThrow<BusinessListException>();
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

        var exception = action.ShouldThrow<CommonException.NotNullException>();
        exception.GetFieldValue!().ShouldNotBeOfType<int>();
        exception.GetFieldValue!().ShouldBeNull();
    }
    
    [Fact]
    public void Accumulate_errors_with_error_accumulation()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();
         
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorRelatedToField;

            validator.Field(_model.NullableInt)
                .WhenNotNull(x => x
                    .Must(_commonFixture.NotBeValid));
            
            validator.Field(_model.Int)
                .Must(_commonFixture.NotBeValid);
        });

        var action = () => parentModelValidator.Validate(_model);

        var exceptions = action.ShouldThrow<BusinessListException>()
            .Exceptions;
        exceptions.Count.ShouldBe(2);
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
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorRelatedToField;

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

        var exceptions = (await action.ShouldThrowAsync<BusinessListException>())
            .Exceptions;
        exceptions.Count.ShouldBe(2);
        exceptions[0].ShouldBeOfType<CommonException.PredicateException>();
        exceptions[0].Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public async Task Accumulate_operations()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();
        var expectedMessage = "Expected message";
        
        var action = () => Validate.That(_model.NullableInt)
            .MustAsync(_commonFixture.BeValidAsync)
            .WhenNotNull(x => x
                .WithMessage("Expected message")
                .MustAsync(_commonFixture.NotBeValidAsync)
                .Must(_commonFixture.ThrowException))
            .Must(_commonFixture.ThrowException)
            .ValidateAsync();
        
        var exception = await action.ShouldThrowAsync<CommonException.PredicateException>();
        exception.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public async Task Nested_WhenNotNull_can_accumulate_operations()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();
        var expectedMessage = "Expected message";
        
        var action = () => Validate.That(_model.NullableInt)
            .MustAsync(_commonFixture.BeValidAsync)
            .WhenNotNull(x => x
                .Transform(x => (int?)x)
                .WhenNotNull(x => x
                    .WithMessage("Expected message")
                    .MustAsync(_commonFixture.NotBeValidAsync))
                .Must(_commonFixture.ThrowException))
            .Must(_commonFixture.ThrowException)
            .ValidateAsync();

        var exception = await action.ShouldThrowAsync<CommonException.PredicateException>();
        exception.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public async Task WhenNotNull_with_nested_WhenNotNull()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();
        var expectedMessage = "Expected message";
        
        var action = () => Validate.That(_model.NullableInt)
            .MustAsync(_commonFixture.BeValidAsync)
            .WhenNotNull(x => x
                .Transform(x => (int?)x)
                .WhenNotNull(x => x
                    .WithMessage(expectedMessage)
                    .Must(_commonFixture.NotBeValid))
                .Must(_commonFixture.ThrowException))
            .Must(_commonFixture.ThrowException)
            .ValidateAsync();

        var exception = await action.ShouldThrowAsync<CommonException.PredicateException>();
        exception.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public async Task WhenNotNull_with_error_accumulation_does_not_continue_the_validation_when_scope_with_operation_accumulated_fails()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();
        
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorRelatedToField;

            validator.Field(_model.NullableInt)
                .WhenNotNull(x => x
                    .MustAsync(_commonFixture.NotBeValidAsync))
                .Must(_commonFixture.ThrowException);
        });

        var action = () => parentModelValidator.ValidateAsync(_model);
        
        await action.ShouldThrowAsync<BusinessException>();
    }
    
    [Fact]
    public async Task WhenNotNull_does_not_continue_the_validation_when_scope_fails()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();
        var expectedMessage = "Expected message";

        var action = () => Validate.That(_model.NullableInt)
            .MustAsync(_commonFixture.BeValidAsync)
            .WhenNotNull(x => x
                .WithMessage("Expected message")
                .Must(_commonFixture.NotBeValid))
            .Must(_commonFixture.ThrowException)
            .ValidateAsync();

        var exception = await action.ShouldThrowAsync<CommonException.PredicateException>();
        exception.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public async Task WhenNotNull_should_not_execute_scope_if_the_nullable_condition_is_not_met()
    {
        var expectedMessage = "Expected message";
        
        var action = () => Validate.That(_model.NullableInt)
            .MustAsync(_commonFixture.BeValidAsync)
            .WhenNotNull<int, int>(_ => throw new Exception())
            .WithMessage(expectedMessage)
            .Must(_commonFixture.NotBeValid)
            .ValidateAsync();

        var exception = await action.ShouldThrowAsync<CommonException.PredicateException>();
        exception.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public async Task WhenNotNull_does_not_fails_with_transformed_field_value()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();
        
        var action = () => Validate.That(_model.NullableInt)
            .WhenAsync(_commonFixture.BeValidAsync)
            .WhenNotNull(x => x
                .Must(x => x == 1))
            .Transform(x => (int?)null)
            .Must(x => x is null)
            .ValidateAsync();

        await action.ShouldNotThrowAsync();
    }

    [Theory]
    [InlineData(ValidationMode.StopOnFirstError)]
    [InlineData(ValidationMode.AccumulateFirstErrorRelatedToField)]
    [InlineData(ValidationMode.AccumulateFirstErrorRelatedToFieldAndFirstErrorOfAllIterations)]
    public async Task Stop_executing_nested_failed_scope_with_previous_scope_with_pending_asynchronous_validation(
        ValidationMode validationMode)
    {
        _model = new ParentModelBuilder()
            .WithNullableNestedModel()
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = validationMode;

            validator.That(_model.NullableNestedModel)
                .WhenNotNull(x => x
                    .MustAsync(_commonFixture.BeValidAsync)
                    .Transform(x => (NestedModel?)x)
                    .WhenNotNull(x => x
                        .MustAsync(_commonFixture.NotBeValidAsync)
                        .Must(_commonFixture.ThrowException))
                    .Must(_commonFixture.ThrowException))
                .Must(_commonFixture.ThrowException);
        });
        
        var action = () => parentModelValidator.ValidateAsync(_model);

        await action.ShouldThrowAsync<BusinessException>();
    }
    
    [Theory]
    [InlineData(ValidationMode.StopOnFirstError, null)]
    [InlineData(ValidationMode.AccumulateFirstErrorRelatedToField, null)]
    [InlineData(ValidationMode.AccumulateFirstErrorRelatedToFieldAndFirstErrorOfAllIterations, 2)]
    public async Task ForEach_works_inside_WhenNotNull(ValidationMode validationMode, int? numberOfExceptions)
    {
        _model = new ParentModelBuilder()
            .WithNullableNestedModel()
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = validationMode;
            
            validator.That(_model.IntListList)
                .WhenNotNull(x => x
                    .MustAsync(_commonFixture.BeValidAsync)
                    .ForEach(x => x
                        .MustAsync(_commonFixture.NotBeValidAsync)
                        .Must(_commonFixture.ThrowException))
                    .Must(_commonFixture.ThrowException))
                .Must(_commonFixture.ThrowException);
        });
        
        var action = () => parentModelValidator.ValidateAsync(_model);

        if (validationMode is ValidationMode.StopOnFirstError or ValidationMode.AccumulateFirstErrorRelatedToField)
        {
            await action.ShouldThrowAsync<BusinessException>();
        }
        else
        {
            var exceptions = (await action.ShouldThrowAsync<BusinessListException>())
                .Exceptions;
            exceptions.Count.ShouldBe(numberOfExceptions!.Value);
        }
    }
}