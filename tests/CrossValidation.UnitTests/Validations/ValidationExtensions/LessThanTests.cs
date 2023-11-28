using Common.Tests;
using CrossValidation.ErrorResources;
using CrossValidation.Exceptions;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

public class LessThanTests : TestBase
{
    [Fact]
    public void Validate_value_is_less_than_comparison_value()
    {
        var value = 1;
        var comparisonValue = 2;
        
        var action = () => Validate.Field(value)
            .LessThan(comparisonValue);

        action.Should()
            .NotThrow();
    }
     
    [Fact]
    public void Fail_when_value_is_not_less_than_comparison_value()
    {
        var value = 1;
        var comparisonValue = 1;
        
        var action = () => Validate.Field(value)
            .LessThan(comparisonValue);

        var exception = action.Should()
            .Throw<CommonException.LessThanException<int>>()
            .Which;
        exception.Code
            .Should()
            .Be(nameof(ErrorResource.LessThan));
        exception.ComparisonValue
            .Should()
            .Be(comparisonValue);
    }
}