﻿using System;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Extensions;
using CrossValidation.Resources;
using CrossValidation.Rules;
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
        var action = () => IValidRule<NestedModel>.CreateFromField( _model.NestedModel)
            .Must(_commonFixture.NotBeValid);

        action.ShouldThrowValidationError<CommonCrossValidationError.Predicate>();
    }
    
    [Fact]
    public void Keep_customizations_before_create_instance()
    {
        var messageTemplate = "{FieldDisplayName}: Expected message";
        var expectedMessage = "NestedModel.Int: Expected message";
        
        var getAge = () => Validate.Field(_model, x => x.NestedModel.Int)
            .WithMessage(messageTemplate)
            .WithError(new CustomErrorWithPlaceholderValue(10))
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
    public void Keep_message_before_create_instance()
    {
        var getAge = () => Validate.Field(_model, x => x.NestedModel.Int)
            .WithMessage(ErrorResource.NotNull)
            .Instance(UserAgeWithoutCustomization.Create);

        var error = getAge.ShouldThrowValidationError<CommonCrossValidationError.GreaterThan<int>>();
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
        
        var error = action.ShouldThrowValidationError<CommonCrossValidationError.Predicate>();
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Keep_instance_customizations()
    {
        var getAge = () => Validate.That(_model.Int)
            .Instance(UserAgeWithCustomization.Create);

        var error = getAge.ShouldThrowValidationError<CommonCrossValidationError.GreaterThan<int>>();
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
                .Instance(UserAgeWithoutCustomization.Create);
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

    private record CustomErrorWithPlaceholderValue(int Value) : CrossValidationError;

    private record UserAgeWithoutCustomization
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
    
    private record UserAgeWithCustomization
    {
        public required int Value { get; set; }
        
        public static UserAgeWithCustomization Create(int value)
        {
            Validate.That(value)
                .WithMessage("Expected message")
                .WithDetails("Expected details")
                .GreaterThan(value + 1);
            return new UserAgeWithCustomization
            {
                Value = value
            };
        }
    }
}