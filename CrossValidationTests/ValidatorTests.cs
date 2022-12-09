using System;
using System.Linq;
using CrossValidation;
using CrossValidation.Validators;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests;

public class ValidatorTests
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
     
     [Fact]
     public void Enum()
     {
         var enumValue = CreateOrderModelProductColor.Red;
         var enumAction = () => Validate.That(enumValue).IsInEnum();
         enumAction.ShouldNotThrow();
         
         var intValue = (int)CreateOrderModelProductColor.Red;
         var intAction = () => Validate.That(intValue)
             .IsInEnum(typeof(CreateOrderModelProductColor));
         intAction.ShouldNotThrow();
         
         var stringValue = nameof(CreateOrderModelProductColor.Red);
         var stringAction = () => Validate.That(stringValue)
             .IsInEnum(typeof(CreateOrderModelProductColor));
         stringAction.ShouldNotThrow();
     }
     
     [Fact]
     public void Enum_fails()
     {
         var lastColorId = (int)System.Enum.GetValues<CreateOrderModelProductColor>().Last();
         var nonExistentColorId = lastColorId + 1;
         
         var enumValue = (CreateOrderModelProductColor)nonExistentColorId;
         var enumAction = () => Validate.That(enumValue).IsInEnum();
         enumAction.ShouldThrow<ValidationException>();
         
         var intValue = nonExistentColorId;
         var intAction = () => Validate.That(intValue)
             .IsInEnum(typeof(CreateOrderModelProductColor));
         intAction.ShouldThrow<ValidationException>();
         
         var stringValue = new Bogus.Faker().Lorem.Sentence();
         var stringAction = () => Validate.That(stringValue)
             .IsInEnum(typeof(CreateOrderModelProductColor));
         stringAction.ShouldThrow<ValidationException>();
     }

     [Fact]
     public void Enum_fails_when_type_is_not_enum()
     {
         var value = (int)CreateOrderModelProductColor.Red;
         var enumAction = () => Validate.That(value)
             .IsInEnum(value.GetType());
         enumAction.ShouldThrow<InvalidOperationException>();
     }
}