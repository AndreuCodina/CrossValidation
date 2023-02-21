using System;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests;

public class CrossValidationOptionsTests
{
    [Fact]
    public void Generate_placeholders_when_they_are_not_added()
    {
        var expectedName = "Value";
        var expectedDateTime = DateTime.UtcNow;
        var originalValue = CrossValidationOptions.GeneratePlaceholderValuesWhenTheyAreNotAdded;
        
        try
        {
            CrossValidationOptions.GeneratePlaceholderValuesWhenTheyAreNotAdded = true;

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
            CrossValidationOptions.GeneratePlaceholderValuesWhenTheyAreNotAdded = originalValue;
        }
    }

    private record ErrorWithFields(string Name, DateTime DateTime) : CrossError;
}