using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.UnitTests.TestUtils;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

public class EqualTests : TestBase
{
    [Fact]
    public void Validate_value_is_equal_to_comparison_value()
    {
        var value = 1;
        var comparisonValue = 1;
        
        var action = () => Validate.Field(value)
            .Equal(comparisonValue);

        action.Should()
            .NotThrow();
    }
     
    [Fact]
    public void Fail_when_value_is_not_equal_to_comparison_value()
    {
        var value = 1;
        var comparisonValue = 2;
        
        var action = () => Validate.Field(value)
            .Equal(comparisonValue);
        
        var exception = action.Should()
            .Throw<CommonException.EqualException<int>>()
            .Which;
        exception.Code
            .Should()
            .Be(nameof(ErrorResource.Equal));
        exception.ComparisonValue
            .Should()
            .Be(comparisonValue);
    }
}