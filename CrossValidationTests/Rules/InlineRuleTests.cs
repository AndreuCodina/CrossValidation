﻿using CrossValidation;
using CrossValidation.Resources;
using CrossValidation.Results;
using CrossValidation.Rules;
using CrossValidationTests.Builders;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Rules;

public class InlineRuleTests
{
    private readonly ParentModel _model;

    public InlineRuleTests()
    {
        _model = new ParentModelBuilder().Build();
    }

    [Fact]
    public void Validator_conditional_execution()
    {
        var expectedMessage = "TrueCase";
        var action = () => new InlineRule<int>(_model.NestedModel.Int)
            .When(x => false)
            .GreaterThan(_model.NestedModel.Int + 1)
            .When(true)
            .WithMessage(expectedMessage)
            .GreaterThan(_model.NestedModel.Int);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Placeholder_values_are_added_automatically_when_they_are_not_added_and_it_is_enabled_in_configuration()
    {
        var defaultConfiguration = CrossValidationConfiguration.GeneratePlaceholderValuesWhenTheyAreNotAdded;
        CrossValidationConfiguration.GeneratePlaceholderValuesWhenTheyAreNotAdded = true;
        
        var action = () => new InlineRule<int>(_model.NestedModel.Int)
            .WithError(new CustomErrorWithPlaceholderValue(_model.NestedModel.Int))
            .GreaterThan(_model.NestedModel.Int);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.PlaceholderValues!
            .ShouldContain(x =>
                x.Key == nameof(CustomErrorWithPlaceholderValue.Value)
                && (int)x.Value == _model.NestedModel.Int);
        
        CrossValidationConfiguration.GeneratePlaceholderValuesWhenTheyAreNotAdded = defaultConfiguration;
    }
    
    [Fact]
    public void Validate_predicate()
    {
        var action = () => new InlineRule<NestedModel>(_model.NestedModel)
            .Must(x => x.Int > x.Int);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.ShouldBeOfType<CommonCrossValidationError.Predicate>();
    }
    
    // [Fact]
    // public void Instance_keeps_customizations()
    // {
    //     var expectedMessage = "Expected message";
    //     var getAge = () => new InlineRule<int>(_model.NestedModel.Int)
    //         .WithMessage(expectedMessage)
    //         .Instance(UserAge.Create);
    //
    //     var ex = getAge.ShouldThrow<CrossValidationException>();
    //     var error = getAge.ShouldThrow<CrossValidationException>().Errors[0];
    //     error.ShouldBeOfType<CommonCrossValidationError.GreaterThan<int>>();
    //     error.Message.ShouldBe(expectedMessage);
    // }
    
    [Fact]
    public void Keep_customizations_before_create_instance()
    {
        var messageTemplate = "{FieldDisplayName}: Expected message";
        var expectedMessage = "NestedModel.Int: Expected message";
        
        var getAge = () => Validate.Field(_model, x => x.NestedModel.Int)
            .WithMessage(messageTemplate)
            .WithError(new CustomErrorWithPlaceholderValue(10))
            .Instance(UserAgeWithoutCustomization.Create);

        var error = getAge.ShouldThrow<CrossValidationException>().Errors[0];
        error.ShouldBeOfType<CustomErrorWithPlaceholderValue>();
        error.FieldName.ShouldBe("NestedModel.Int");
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Keep_localized_message_before_create_instance()
    {
        var getAge = () => Validate.Field(_model, x => x.NestedModel.Int)
            .WithCode(nameof(ErrorResource.NotNull))
            .Instance(UserAgeWithoutCustomization.Create);

        var error = getAge.ShouldThrow<CrossValidationException>().Errors[0];
        error.ShouldBeOfType<CommonCrossValidationError.GreaterThan<int>>();
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
        error.Message.ShouldBe(ErrorResource.NotNull);
    }
    
    [Fact]
    public void Keep_instance_customizations()
    {
        var getAge = () => Validate.Field(_model, x => x.NestedModel.Int)
            .Instance(UserAgeWithCustomization.Create);

        var error = getAge.ShouldThrow<CrossValidationException>().Errors[0];
        error.ShouldBeOfType<CommonCrossValidationError.GreaterThan<int>>();
        error.Code.ShouldBe(nameof(ErrorResource.GreaterThan));
        error.Message.ShouldBe("Expected message");
    }

    public record CustomErrorWithPlaceholderValue(int Value) : CrossValidationError;

    public record UserAgeWithoutCustomization
    {
        public required int Value { get; set; }
        
        public static UserAgeWithoutCustomization Create(int value)
        {
            Validate.That(value).GreaterThan(int.MaxValue);
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
                .GreaterThan(int.MaxValue);
            return new UserAgeWithCustomization
            {
                Value = value
            };
        }
    }
}