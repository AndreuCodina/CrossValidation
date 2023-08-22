using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Validations;
using FluentAssertions;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

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
        
        action.Should()
            .Throw<CommonException.LessThanException<int>>()
            .And
            .Code
            .ShouldBe(nameof(ErrorResource.LessThan));
    }
}