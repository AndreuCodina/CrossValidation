using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Validations;
using FluentAssertions;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

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
        
        action.Should()
            .Throw<CommonException.GreaterThanException<int>>()
            .And
            .Code
            .ShouldBe(nameof(ErrorResource.GreaterThan));
    }
}