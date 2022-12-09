using System;
using System.Linq;
using CrossValidation;
using CrossValidation.Rules;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Validators;

public class EnumValidatorTests
{
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