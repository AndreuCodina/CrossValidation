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
        var originalValue = CrossValidationOptions.GeneratePlaceholderValuesWhenTheyAreNotAdded;

        try
        {
            CrossValidationOptions.GeneratePlaceholderValuesWhenTheyAreNotAdded = true;

            try
            {
                throw new ErrorWithFields("Value").ToException();
            }
            catch (CrossException e)
            {
                e.Error.PlaceholderValues.ShouldNotBeEmpty();
            }
        }
        finally
        {
            CrossValidationOptions.GeneratePlaceholderValuesWhenTheyAreNotAdded = originalValue;
        }
    }

    private record ErrorWithFields(string Name) : CrossError;
}