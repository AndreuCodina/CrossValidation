using System;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Rules;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.Builders;
using CrossValidation.Tests.Fixtures;
using CrossValidation.Tests.Models;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Rules;

public class RuleTests : IClassFixture<CommonFixture>
{
    private readonly CommonFixture _commonFixture;
    private ParentModel _model;

    public RuleTests(CommonFixture commonFixture)
    {
        _commonFixture = commonFixture;
        _model = new ParentModelBuilder().Build();
    }

    [Fact]
    public void Placeholder_values_are_added_automatically_when_they_are_not_added_and_it_is_enabled_in_configuration()
    {
        var defaultConfiguration = CrossValidationConfiguration.GeneratePlaceholderValuesWhenTheyAreNotAdded;
        CrossValidationConfiguration.GeneratePlaceholderValuesWhenTheyAreNotAdded = true;
        
        var action = () => IValidRule<int>.CreateFromField(_model.NestedModel.Int)
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
        var action = () => IValidRule<NestedModel>.CreateFromField(_model.NestedModel)
            .Must(_commonFixture.NotBeValid);

        action.ShouldThrowValidationError<CommonCodeValidationError.Predicate>();
    }
    
    [Fact]
    public void Keep_customizations_before_create_instance()
    {
        var messageTemplate = "{FieldDisplayName}: Expected message";
        var expectedMessage = "NestedModel.Int: Expected message";
        
        var getAge = () => Validate.Field(_model, x => x.NestedModel.Int)
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
        var getAge = () => Validate.Field(_model, x => x.NestedModel.Int)
            .WithCode(nameof(ErrorResource.NotNull))
            .Instance(ValueObjectWithoutCustomization.Create);

        var error = getAge.ShouldThrowValidationError<CommonCodeValidationError.GreaterThan<int>>();
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
        error.Message.ShouldBe(ErrorResource.NotNull);
    }
    
    [Fact]
    public void Keep_message_before_create_instance()
    {
        var getAge = () => Validate.Field(_model, x => x.NestedModel.Int)
            .WithMessage(ErrorResource.NotNull)
            .Instance(ValueObjectWithoutCustomization.Create);

        var error = getAge.ShouldThrowValidationError<CommonCodeValidationError.GreaterThan<int>>();
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
        
        var error = action.ShouldThrowValidationError<CommonCodeValidationError.Predicate>();
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Keep_instance_customizations()
    {
        var getAge = () => Validate.That(_model.Int)
            .Instance(ValueObjectWithCustomization.Create);

        var error = getAge.ShouldThrowValidationError<CommonCodeValidationError.GreaterThan<int>>();
        error.Code.ShouldBe(nameof(ErrorResource.GreaterThan));
        error.Message.ShouldBe("Expected message");
        error.Details.ShouldBe("Expected details");
    }
    
    [Fact]
    public void Call_Instance_from_invalid_rule_fails()
    {
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachRule;

            validator.RuleFor(x => x.NullableInt)
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
    public void Validator_with_conditional_execution()
    {
        var expectedMessage = "TrueCase";
        var action = () => IValidRule<int>.CreateFromField(_model.NestedModel.Int)
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
        var action = () => IValidRule<int>.CreateFromField(_model.NestedModel.Int)
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

        action.ShouldThrow<CrossValidationException>();
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

        action.ShouldThrow<CrossValidationException>();
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
            .WithError(new CommonCodeValidationError.NotNull())
            .WithError(new CommonCodeValidationError.Enum())
            .Must(_commonFixture.NotBeValid);

        var error = action.ShouldThrowValidationError<CommonCodeValidationError.Enum>();
        error.Code.ShouldBe(nameof(ErrorResource.Enum));
        error.Message.ShouldBe(ErrorResource.Enum);
    }
    
    [Fact]
    public void Validators_do_not_override_customization()
    {
        var expectedDetails = "Expected details";
        var action = () => Validate.That(_model.Int)
            .WithCode(nameof(ErrorResource.NotNull))
            .WithDetails(expectedDetails)
            .GreaterThan(_model.Int);

        var error = action.ShouldThrowValidationError<CommonCodeValidationError.GreaterThan<int>>();
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
        error.Message.ShouldBe(ErrorResource.NotNull);
        error.Details.ShouldBe(expectedDetails);
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
    }

    private record CustomErrorWithPlaceholderValue(int Value) : ValidationError;

    private record ValueObjectWithoutCustomization
    {
        public required int Value { get; set; }
        
        public static ValueObjectWithoutCustomization Create(int value)
        {
            Validate.That(value).GreaterThan(value + 1);
            return new ValueObjectWithoutCustomization
            {
                Value = value
            };
        }
    }
    
    private record ValueObjectWithCustomization
    {
        public required int Value { get; set; }
        
        public static ValueObjectWithCustomization Create(int value)
        {
            Validate.That(value)
                .WithMessage("Expected message")
                .WithDetails("Expected details")
                .GreaterThan(value + 1);
            return new ValueObjectWithCustomization
            {
                Value = value
            };
        }
    }

    private record ErrorWithCustomization() : ValidationError(
        Code: nameof(CommonCodeValidationError.NotNull),
        Details: "Expected details");
}