using Common.Tests;
using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

public class GreaterThanOrEqualTests : TestBase
{
    [Theory]
    [InlineData(2, 1)]
    [InlineData(1, 1)]
    public void Validate_value_is_greater_than_or_equal_to_comparison_value(int value, int comparisonValue)
    {
        var action = () => Validate.Field(value)
            .GreaterThanOrEqual(comparisonValue);

        action.Should()
            .NotThrow();
    }

    [Fact]
    public void Fail_when_value_is_not_greater_than_or_equal_to_comparison_value()
    {
        var value = 1;
        var comparisonValue = 2;
        
        var action = () => Validate.Field(value)
            .GreaterThanOrEqual(comparisonValue);
        
        var exception = action.Should()
            .Throw<CommonException.GreaterThanOrEqualException<int>>()
            .Which;
        exception.Code
            .Should()
            .Be(nameof(ErrorResource.GreaterThanOrEqual));
        exception.ComparisonValue
            .Should()
            .Be(comparisonValue);
    }
}