using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Validations;
using FluentAssertions;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

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

        action.Should()
            .Throw<CommonException.GreaterThanOrEqualException<int>>()
            .And
            .Code
            .ShouldBe(nameof(ErrorResource.GreaterThanOrEqual));
    }
}