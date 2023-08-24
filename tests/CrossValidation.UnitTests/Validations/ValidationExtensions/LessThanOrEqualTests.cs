using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.UnitTests.TestUtils;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

public class LessThanOrEqualTests : TestBase
{
    [Theory]
    [InlineData(1, 2)]
    [InlineData(1, 1)]
    public void Validate_value_is_less_than_or_equal_to_comparison_value(int value, int comparisonValue)
    {
        var action = () => Validate.Field(value)
            .LessThanOrEqual(comparisonValue);

        action.Should()
            .NotThrow();
    }

    [Fact]
    public void Fail_when_value_is_not_less_than_or_equal_to_comparison_value()
    {
        var value = 2;
        var comparisonValue = 1;
        
        var action = () => Validate.Field(value)
            .LessThanOrEqual(comparisonValue);

        var exception = action.Should()
            .Throw<CommonException.LessThanOrEqualException<int>>()
            .Which;
        exception.Code
            .Should()
            .Be(nameof(ErrorResource.LessThanOrEqual));
        exception.ComparisonValue
            .Should()
            .Be(comparisonValue);
    }
}