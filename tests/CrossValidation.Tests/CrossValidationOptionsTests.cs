using System;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests;

public class CrossValidationOptionsTests :
    TestBase,
    IDisposable
{
    private ParentModel _model;

    public CrossValidationOptionsTests()
    {
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void Generate_placeholders_when_they_are_not_added()
    {
        var expectedName = "Value";
        var expectedDateTime = DateTime.UtcNow;
        var originalValue = CrossValidationOptions.LocalizeErrorInClient;
        
        try
        {
            CrossValidationOptions.LocalizeErrorInClient = true;

            try
            {
                throw new ErrorWithFields(expectedName, expectedDateTime).ToException();
            }
            catch (CrossException e)
            {
                e.Error.PlaceholderValues.ShouldNotBeEmpty();
                e.Error.PlaceholderValues.Keys.Count.ShouldBe(2);
                e.Error.PlaceholderValues[nameof(ErrorWithFields.Name)].ShouldBe(expectedName);
                e.Error.PlaceholderValues[nameof(ErrorWithFields.DateTime)].ShouldBe(expectedDateTime);
            }
        }
        finally
        {
            CrossValidationOptions.LocalizeErrorInClient = originalValue;
        }
    }
    
    [Fact]
    public void Placeholder_values_are_added_automatically_when_they_are_not_added_and_it_is_enabled_in_configuration()
    {
        var defaultConfiguration = CrossValidationOptions.LocalizeErrorInClient;
        CrossValidationOptions.LocalizeErrorInClient = true;
        
        var action = () => Validate.That(_model.NestedModel.Int)
            .WithError(new CustomErrorWithPlaceholderValue(_model.NestedModel.Int))
            .GreaterThan(_model.NestedModel.Int);

        var error = action.ShouldThrowCrossError();
        error.PlaceholderValues!
            .ShouldContain(x =>
                x.Key == nameof(CustomErrorWithPlaceholderValue.Value)
                && (int)x.Value == _model.NestedModel.Int);
        
        CrossValidationOptions.LocalizeErrorInClient = defaultConfiguration;
    }

    private record ErrorWithFields(string Name, DateTime DateTime) : CrossError;
    
    private record CustomErrorWithPlaceholderValue(int Value) : CrossError;

    public void Dispose()
    {
        CrossValidationOptions.SetDefaultOptions();
    }
}