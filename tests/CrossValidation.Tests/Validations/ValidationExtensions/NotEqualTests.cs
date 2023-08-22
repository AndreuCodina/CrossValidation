using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Validations;
using FluentAssertions;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

public class nOTEqualTests : TestBase
{
    [Fact]
    public void Validate_value_is_not_equal_to_comparison_value()
    {
        var value = 1;
        var comparisonValue = 2;
        var action = () => Validate.Field(value)
            .NotEqual(comparisonValue);

        action.Should()
            .NotThrow();
    }
     
    [Fact]
    public void Fail_when_value_is_equal_to_comparison_value()
    {
        var value = 1;
        var comparisonValue = 1;
        var action = () => Validate.Field(value)
            .NotEqual(comparisonValue);
        
        action.Should()
            .Throw<CommonException.NotEqualException<int>>()
            .And
            .Code
            .ShouldBe(nameof(ErrorResource.NotEqual));
    }
}