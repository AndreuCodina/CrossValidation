using CrossValidation.Validators;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Validators;

public class GreaterThanValidatorTests
{
    [Fact]
    public void Validate_greater_than()
    {
        var intValue = 2;
        var comparisonIntValue = 1;
        var intValidator = new GreaterThanValidator<int>(FieldValue: intValue, ComparisonValue: comparisonIntValue);
        var isValid = intValidator.IsValid();
        isValid.ShouldBeTrue();
         
        var floatValue = 2f;
        var comparisonFloatValue = 1f;
        var floatValidator = new GreaterThanValidator<float>(FieldValue: floatValue, ComparisonValue: comparisonFloatValue);
        isValid = floatValidator.IsValid();
        isValid.ShouldBeTrue();
         
        var decimalValue = 2m;
        var comparisonDecimalValue = 1m;
        var decimalValidator = new GreaterThanValidator<decimal>(FieldValue: decimalValue, ComparisonValue: comparisonDecimalValue);
        isValid = decimalValidator.IsValid();
        isValid.ShouldBeTrue();
    }
     
    [Fact]
    public void Validate_greater_than_fails()
    {
        var intValue = 1;
        var comparisonIntValue = 2;
        var intValidator = new GreaterThanValidator<int>(FieldValue: intValue, ComparisonValue: comparisonIntValue);
        var isValid = intValidator.IsValid();
        isValid.ShouldBeFalse();
         
        var floatValue = 1f;
        var comparisonFloatValue = 2f;
        var floatValidator = new GreaterThanValidator<float>(FieldValue: floatValue, ComparisonValue: comparisonFloatValue);
        isValid = floatValidator.IsValid();
        isValid.ShouldBeFalse();
         
        var decimalValue = 1m;
        var comparisonDecimalValue = 2m;
        var decimalValidator = new GreaterThanValidator<decimal>(FieldValue: decimalValue, ComparisonValue: comparisonDecimalValue);
        isValid = decimalValidator.IsValid();
        isValid.ShouldBeFalse();
    }
}