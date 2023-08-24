using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.UnitTests.TestUtils;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

public class GreaterThanTests : TestBase
{
    [Fact]
    public void Validate_value_is_greater_than_comparison_value()
    {
        var value = 2;
        var comparisonValue = 1;
        
        var action = () => Validate.Field(value)
            .GreaterThan(comparisonValue);

        action.Should()
            .NotThrow();
    }
     
    [Fact]
    public void Fail_when_value_is_not_greater_than_comparison_value()
    {
        var value = 1;
        var comparisonValue = 1;
        
        var action = () => Validate.Field(value)
            .GreaterThan(comparisonValue);
        
        var exception = action.Should()
            .Throw<CommonException.GreaterThanException<int>>()
            .Which;
        exception.Code
            .Should()
            .Be(nameof(ErrorResource.GreaterThan));
        exception.ComparisonValue
            .Should()
            .Be(comparisonValue);
    }
}