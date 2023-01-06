using System;
using CrossValidation;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Extensions;
using CrossValidation.Resources;
using CrossValidation.Rules;
using CrossValidationTests.Builders;
using CrossValidationTests.Fixtures;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Rules;

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
        
        var action = () => Rule<int>.CreateFromField(() => _model.NestedModel.Int, RuleState.Valid)
            .WithError(() => new CustomErrorWithPlaceholderValue(_model.NestedModel.Int))
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
        var action = () => Rule<NestedModel>.CreateFromField(() => _model.NestedModel, RuleState.Valid)
            .Must(x => x.Int > x.Int);

        action.ShouldThrowValidationError<CommonCrossValidationError.Predicate>();
    }
    
    [Fact]
    public void Keep_customizations_before_create_instance()
    {
        var messageTemplate = "{FieldDisplayName}: Expected message";
        var expectedMessage = "NestedModel.Int: Expected message";
        
        var getAge = () => Validate.Field(_model, x => x.NestedModel.Int)
            .WithMessage(messageTemplate)
            .WithError(() => new CustomErrorWithPlaceholderValue(10))
            .Instance(UserAgeWithoutCustomization.Create);

        var error = getAge.ShouldThrowValidationError<CustomErrorWithPlaceholderValue>();
        error.FieldName.ShouldBe("NestedModel.Int");
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Keep_localized_message_before_create_instance()
    {
        var getAge = () => Validate.Field(_model, x => x.NestedModel.Int)
            .WithCode(nameof(ErrorResource.NotNull))
            .Instance(UserAgeWithoutCustomization.Create);

        var error = getAge.ShouldThrowValidationError<CommonCrossValidationError.GreaterThan<int>>();
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
        error.Message.ShouldBe(ErrorResource.NotNull);
    }
    
    [Fact]
    public void Keep_instance_customizations()
    {
        var getAge = () => Validate.Field(_model, x => x.NestedModel.Int)
            .Instance(UserAgeWithCustomization.Create);

        var error = getAge.ShouldThrowValidationError<CommonCrossValidationError.GreaterThan<int>>();
        error.Code.ShouldBe(nameof(ErrorResource.GreaterThan));
        error.Message.ShouldBe("Expected message");
        error.FieldDisplayName.ShouldBe("Expected field display name");
    }
    
    [Fact]
    public void Call_Instance_from_model_validator_with_error_accumulation_fails()
    {
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachRule;

            validator.RuleFor(x => x.NullableInt)
                .NotNull()
                .Transform(x => x + 1)
                .Transform(x => x.ToString())
                .Transform(int.Parse)
                .Instance(UserAgeWithoutCustomization.Create);
        });
    
        var action = () => parentModelValidator.Validate(_model);
    
        action.ShouldThrow<InvalidOperationException>();
    }
    
    [Fact]
    public void Validator_with_conditional_execution()
    {
        var expectedMessage = "TrueCase";
        var action = () => Rule<int>.CreateFromField(() => _model.NestedModel.Int, RuleState.Valid)
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
        var action = () => Rule<int>.CreateFromField(() => _model.NestedModel.Int, RuleState.Valid)
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

    public record CustomErrorWithPlaceholderValue(int Value) : CrossValidationError;

    public record UserAgeWithoutCustomization
    {
        public required int Value { get; set; }
        
        public static UserAgeWithoutCustomization Create(int value)
        {
            Validate.That(value).GreaterThan(value + 1);
            return new UserAgeWithoutCustomization
            {
                Value = value
            };
        }
    }
    
    public record UserAgeWithCustomization
    {
        public required int Value { get; set; }
        
        public static UserAgeWithCustomization Create(int value)
        {
            Validate.That(value)
                .WithMessage("Expected message")
                .WithFieldDisplayName("Expected field display name")
                .GreaterThan(value + 1);
            return new UserAgeWithCustomization
            {
                Value = value
            };
        }
    }
}