using CrossValidation.FieldValidators;
using Shouldly;
using Xunit;

namespace CrossValidationTests;

public class FieldValidatorTests
{
    [Fact]
    public void NotNull()
    {
        var value = new Bogus.Faker().Lorem.Word();
        var validator = new NotNullValidator<string>(value);

        var isValid = validator.IsValid();

        isValid.ShouldBeTrue();
    }
    
    [Fact]
    public void NotNull_fails()
    {
        string? value = null;
        var validator = new NotNullValidator<string?>(value);

        var isValid = validator.IsValid();

        isValid.ShouldBeFalse();
    }
    
    [Fact]
    public void Null()
    {
        string? value = null;
        var validator = new NullValidator<string?>(value);

        var isValid = validator.IsValid();

        isValid.ShouldBeTrue();
    }
    
    [Fact]
    public void Null_fails()
    {
        var value = new Bogus.Faker().Lorem.Word();
        var validator = new NullValidator<string>(value);

        var isValid = validator.IsValid();

        isValid.ShouldBeFalse();
    }
     
     [Fact]
     public void GreaterThan()
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
     public void GreaterThanFails()
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