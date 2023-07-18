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
    public void Not_generate_placeholders_throwing_without_DSL_when_they_are_not_added()
    {
        var expectedName = "Value";
        var expectedDateTime = DateTime.UtcNow;
        var originalValue = CrossValidationOptions.LocalizeErrorInClient;
        
        try
        {
            CrossValidationOptions.LocalizeErrorInClient = true;

            try
            {
                throw new ErrorWithFields(expectedName, expectedDateTime);
            }
            catch (BusinessException e)
            {
                e.PlaceholderValues.ShouldNotBeEmpty();
                e.PlaceholderValues[nameof(ErrorWithFields.Name)].ShouldBe(expectedName);
                e.PlaceholderValues[nameof(ErrorWithFields.DateTime)].ShouldBe(expectedDateTime);
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
            .WithException(new CustomErrorWithPlaceholderValue(_model.NestedModel.Int))
            .GreaterThan(_model.NestedModel.Int);

        var error = action.ShouldThrowCrossError();
        error.PlaceholderValues!
            .ShouldContain(x =>
                x.Key == nameof(CustomErrorWithPlaceholderValue.Value)
                && (int)x.Value == _model.NestedModel.Int);
        
        CrossValidationOptions.LocalizeErrorInClient = defaultConfiguration;
    }

    private class ErrorWithFields(string name, DateTime dateTime) : BusinessException
    {
        public string Name => name;
        public DateTime DateTime => dateTime;
    }
    
    private class CustomErrorWithPlaceholderValue(int value) : BusinessException
    {
        public int Value => value;
    }

    public void Dispose()
    {
        CrossValidationOptions.SetDefaultOptions();
    }
}