using System;
using System.Net;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.Builders;
using CrossValidation.Tests.Fixtures;
using CrossValidation.Tests.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations;

public class ValidationTests : IClassFixture<CommonFixture>
{
    private readonly CommonFixture _commonFixture;
    private ParentModel _model;

    public ValidationTests(CommonFixture commonFixture)
    {
        _commonFixture = commonFixture;
        _model = new ParentModelBuilder().Build();
    }

    [Fact]
    public void Placeholder_values_are_added_automatically_when_they_are_not_added_and_it_is_enabled_in_configuration()
    {
        var defaultConfiguration = CrossValidationConfiguration.GeneratePlaceholderValuesWhenTheyAreNotAdded;
        CrossValidationConfiguration.GeneratePlaceholderValuesWhenTheyAreNotAdded = true;
        
        var action = () => Validate.That(_model.NestedModel.Int)
            .WithError(new CustomErrorWithPlaceholderValue(_model.NestedModel.Int))
            .GreaterThan(_model.NestedModel.Int);

        var error = action.ShouldThrowValidationError();
        error.PlaceholderValues!
            .ShouldContain(x =>
                x.Key == nameof(CustomErrorWithPlaceholderValue.Value)
                && (int)x.Value == _model.NestedModel.Int);
        
        CrossValidationConfiguration.GeneratePlaceholderValuesWhenTheyAreNotAdded = defaultConfiguration;
    }
    
    [Fact]
    public void Validate_predicate()
    {
        var action = () => Validate.That(_model.NestedModel)
            .Must(_commonFixture.NotBeValid);

        action.ShouldThrowValidationError<CommonCrossError.Predicate>();
    }
    
    [Fact]
    public void Keep_customizations_before_create_instance()
    {
        var messageTemplate = "{FieldDisplayName}: Expected message";
        var expectedMessage = "NestedModel.Int: Expected message";
        
        var getAge = () => Validate.Field(_model.NestedModel.Int)
            .WithMessage(messageTemplate)
            .WithError(new CustomErrorWithPlaceholderValue(10))
            .Instance(ValueObjectWithoutCustomization.Create);

        var error = getAge.ShouldThrowValidationError<CustomErrorWithPlaceholderValue>();
        error.FieldName.ShouldBe("NestedModel.Int");
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Keep_localized_message_before_create_instance()
    {
        var getAge = () => Validate.Field(_model.NestedModel.Int)
            .WithCode(nameof(ErrorResource.NotNull))
            .Instance(ValueObjectWithoutCustomization.Create);

        var error = getAge.ShouldThrowValidationError<CommonCrossError.GreaterThan<int>>();
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
        error.Message.ShouldBe(ErrorResource.NotNull);
    }
    
    [Fact]
    public void Keep_message_before_create_instance()
    {
        var getAge = () => Validate.Field(_model.NestedModel.Int)
            .WithMessage(ErrorResource.NotNull)
            .Instance(ValueObjectWithoutCustomization.Create);

        var error = getAge.ShouldThrowValidationError<CommonCrossError.GreaterThan<int>>();
        error.Message.ShouldBe(ErrorResource.NotNull);
    }
    
    [Fact]
    public void Keep_message_inside_create_instance()
    {
        var expectedMessage = "Expected message";
        
        var action = () => Validate.That(_model.String)
            .Instance(x =>
            {
                Validate.That(x)
                    .WithMessage(expectedMessage)
                    .Must(_commonFixture.NotBeValid);
                return x;
            });
        
        var error = action.ShouldThrowValidationError<CommonCrossError.Predicate>();
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Keep_instance_customizations()
    {
        var getAge = () => Validate.That(_model.Int)
            .Instance(ValueObjectWithCustomization.Create);

        var error = getAge.ShouldThrowValidationError<CommonCrossError.GreaterThan<int>>();
        error.Code.ShouldBe(nameof(ErrorResource.GreaterThan));
        error.Message.ShouldBe("Expected message");
        error.Details.ShouldBe("Expected details");
        error.HttpStatusCode.ShouldBe(HttpStatusCode.Accepted);
        error.FieldDisplayName.ShouldBe("Expected field display name");
    }
    
    [Fact]
    public void Call_Instance_from_invalid_validation_fails()
    {
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachValidation;

            validator.Field(_model.NullableInt)
                .NotNull()
                .Transform(x => x + 1)
                .Transform(x => x.ToString())
                .Transform(int.Parse)
                .Instance();
        });
    
        var action = () => parentModelValidator.Validate(_model);
    
        action.ShouldThrow<InvalidOperationException>();
    }
    
    [Fact]
    public void Call_Instance_with_function_from_invalid_validation_fails()
    {
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachValidation;

            validator.Field(_model.NullableInt)
                .NotNull()
                .Transform(x => x + 1)
                .Transform(x => x.ToString())
                .Transform(int.Parse)
                .Instance(ValueObjectWithoutCustomization.Create);
        });
    
        var action = () => parentModelValidator.Validate(_model);
    
        action.ShouldThrow<InvalidOperationException>();
    }
    
    [Fact]
    public void Get_instance()
    {
        Validate.That(_model.Int)
            .Instance()
            .ShouldBe(_model.Int);

        Validate.That(_model.NullableInt)
            .Instance()
            .ShouldBe(_model.NullableInt);
    }
    
    [Fact]
    public void Get_transformed_instance()
    {
        var expectedValue = 1;
        _model = new ParentModelBuilder()
            .WithNullableInt(expectedValue)
            .Build();

        Validate.That(_model.NullableInt)
            .NotNull()
            .Instance()
            .ShouldBe(expectedValue);
        
        Validate.That(_model.Int)
            .Transform(_ => expectedValue)
            .Instance()
            .ShouldBe(expectedValue);
        
        Validate.That(nameof(ParentModelEnum.Red))
            .Enum<ParentModelEnum>()
            .Instance()
            .ShouldBe(ParentModelEnum.Red);
    }

    [Fact]
    public void Validator_with_conditional_execution()
    {
        var expectedMessage = "TrueCase";
        var action = () => Validate.That(_model.NestedModel.Int)
            .When(_commonFixture.NotBeValid)
            .GreaterThan(_model.NestedModel.Int + 1)
            .When(true)
            .WithMessage(expectedMessage)
            .GreaterThan(_model.NestedModel.Int);

        var error = action.ShouldThrowValidationError();
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Validator_with_async_conditional_execution()
    {
        var expectedMessage = "TrueCase";
        var action = () => Validate.That(_model.NestedModel.Int)
            .WhenAsync(_commonFixture.NotBeValidAsync)
            .GreaterThan(_model.NestedModel.Int + 1)
            .WhenAsync(_commonFixture.BeValidAsync)
            .WithMessage(expectedMessage)
            .GreaterThan(_model.NestedModel.Int);

        var error = action.ShouldThrowValidationError();
        error.Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public void Must()
    {
        var action = () => Validate.That(1)
            .Must(x => x == 1);

        action.ShouldNotThrow();
    }

    [Fact]
    public void Must_fails()
    {
        var action = () => Validate.That(1)
            .Must(x => x != 1);

        action.ShouldThrow<CrossException>();
    }
    
    [Fact]
    public void MustAsync()
    {
        var action = () => Validate.That(1)
            .MustAsync(_commonFixture.BeValidAsync);

        action.ShouldNotThrow();
    }
    
    [Fact]
    public void MustAsync_fails()
    {
        var action = () => Validate.That(1)
            .MustAsync(_commonFixture.NotBeValidAsync);

        action.ShouldThrow<CrossException>();
    }
    
    [Fact]
    public void Repeat_customization_applies_new_customization()
    {
        var expectedMessage = "Expected message";

        var action = () => Validate.That(_model.NullableInt)
            .WithMessage("Old message")
            .WithMessage(expectedMessage)
            .NotNull();

        var error = action.ShouldThrowValidationError();
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Repeat_error_customization_applies_new_error()
    {
        var action = () => Validate.That(_model.NullableInt)
            .WithError(new CommonCrossError.NotNull())
            .WithError(new CommonCrossError.Enum())
            .Must(_commonFixture.NotBeValid);

        var error = action.ShouldThrowValidationError<CommonCrossError.Enum>();
        error.Code.ShouldBe(nameof(ErrorResource.Enum));
        error.Message.ShouldBe(ErrorResource.Enum);
    }
    
    [Fact]
    public void Validators_do_not_override_customization()
    {
        var expectedDetails = "Expected details";
        var expectedHttpStatusCode = HttpStatusCode.Created;
        var action = () => Validate.That(_model.Int)
            .WithCode(nameof(ErrorResource.NotNull))
            .WithDetails(expectedDetails)
            .WithHttpStatusCode(HttpStatusCode.Created)
            .GreaterThan(_model.Int);

        var error = action.ShouldThrowValidationError<CommonCrossError.GreaterThan<int>>();
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
        error.Message.ShouldBe(ErrorResource.NotNull);
        error.Details.ShouldBe(expectedDetails);
        error.HttpStatusCode.ShouldBe(expectedHttpStatusCode);
    }
    
    [Fact]
    public void Validators_do_not_override_error_customization()
    {
        var expectedDetails = "Expected details";
        var action = () => Validate.That(_model.Int)
            .WithError(new ErrorWithCustomization())
            .GreaterThan(_model.Int);

        var error = action.ShouldThrowValidationError<ErrorWithCustomization>();
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
        error.Message.ShouldBe(ErrorResource.NotNull);
        error.Details.ShouldBe(expectedDetails);
        error.HttpStatusCode.ShouldBe(HttpStatusCode.Created);
    }

    private record CustomErrorWithPlaceholderValue(int Value) : CrossError;

    private record ValueObjectWithoutCustomization(int Value)
    {
        public static ValueObjectWithoutCustomization Create(int value)
        {
            Validate.That(value).GreaterThan(value + 1);
            return new(value);
        }
    }

    private record ValueObjectWithCustomization(int Value)
    {
        public static ValueObjectWithCustomization Create(int value)
        {
            Validate.That(value)
                .WithMessage("Expected message")
                .WithDetails("Expected details")
                .WithHttpStatusCode(HttpStatusCode.Accepted)
                .WithFieldDisplayName("Expected field display name")
                .GreaterThan(value + 1);
            return new(value);
        }
    }

    private record ErrorWithCustomization() : CrossError(
        Code: nameof(CommonCrossError.NotNull),
        Details: "Expected details",
        HttpStatusCode: System.Net.HttpStatusCode.Created);
}