using System;
using System.Linq;
using CrossValidation;
using CrossValidation.Exceptions;
using CrossValidation.Rules;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Validators;

public class EnumValidatorTests
{
    [Fact]
    public void Validate_enumeration()
    {
        var enumValue = NestedEnum.Red;
        var enumAction = () => Validate.That(enumValue)
            .IsInEnum();
        enumAction.ShouldNotThrow();
         
        var intValue = (int)NestedEnum.Red;
        var intAction = () => Validate.That(intValue)
            .IsInEnum(typeof(NestedEnum));
        intAction.ShouldNotThrow();
         
        var stringValue = nameof(NestedEnum.Red);
        var stringAction = () => Validate.That(stringValue)
            .IsInEnum(typeof(NestedEnum));
        stringAction.ShouldNotThrow();
    }
     
    [Fact]
    public void Validate_enumeration_fails()
    {
        var lastColorId = (int)System.Enum.GetValues<NestedEnum>().Last();
        var nonExistentColorId = lastColorId + 1;
         
        var enumValue = (NestedEnum)nonExistentColorId;
        var enumAction = () => Validate.That(enumValue)
            .IsInEnum();
        enumAction.ShouldThrow<CrossValidationException>();
         
        var intValue = nonExistentColorId;
        var intAction = () => Validate.That(intValue)
            .IsInEnum(typeof(NestedEnum));
        intAction.ShouldThrow<CrossValidationException>();
         
        var stringValue = new Bogus.Faker().Lorem.Sentence();
        var stringAction = () => Validate.That(stringValue)
            .IsInEnum(typeof(NestedEnum));
        stringAction.ShouldThrow<CrossValidationException>();
    }

    [Fact]
    public void Validate_enumeration_fails_when_type_is_not_enumeration()
    {
        var value = (int)NestedEnum.Red;
        var enumAction = () => Validate.That(value)
            .IsInEnum(value.GetType());
        enumAction.ShouldThrow<InvalidOperationException>();
    }
}