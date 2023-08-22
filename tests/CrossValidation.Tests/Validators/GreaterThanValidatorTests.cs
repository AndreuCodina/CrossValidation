using CrossValidation.Tests.TestUtils;
using CrossValidation.Validators.ComparisonValidators;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validators;

public class GreaterThanValidatorTests : TestBase
{
    [Fact]
    public void Validate_value_is_greater_than_comparison_value()
    {
        var value = 2;
        var comparisonValue = 1;
        var validator = new GreaterThanValidator<int>(value, comparisonValue);
        var isValid = validator.IsValid();
        isValid.ShouldBeTrue();
    }
     
    [Fact]
    public void Fail_when_value_is_not_greater_than_comparison_value()
    {
        var value = 1;
        var comparisonValue = 2;
        var validator = new GreaterThanValidator<int>(value, comparisonValue);
        var isValid = validator.IsValid();
        isValid.ShouldBeFalse();
    }
}