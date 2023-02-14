using System;
using System.Linq;
using CrossValidation.Exceptions;
using CrossValidation.Rules;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.Models;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validators;

public class EnumValidatorTests
{
    [Fact]
    public void Validate_enumeration()
    {
        var enumValue = ParentModelEnum.Red;
        var enumAction = () => Validate.That(enumValue)
            .Enum();
        enumAction.ShouldNotThrow();
         
        var intValue = (int)ParentModelEnum.Red;
        var intAction = () => Validate.That(intValue)
            .Enum<ParentModelEnum>();
        intAction.ShouldNotThrow();
         
        var stringValue = nameof(ParentModelEnum.Red);
        var stringAction = () => Validate.That(stringValue)
            .Enum<ParentModelEnum>();
        stringAction.ShouldNotThrow();
    }
     
    [Fact]
    public void Validate_enumeration_fails()
    {
        var lastColorId = (int)Enum.GetValues<ParentModelEnum>().Last();
        var nonExistentColorId = lastColorId + 1;
         
        var enumValue = (ParentModelEnum)nonExistentColorId;
        var enumAction = () => Validate.That(enumValue)
            .Enum();
        enumAction.ShouldThrowValidationError();
         
        var intValue = nonExistentColorId;
        var intAction = () => Validate.That(intValue)
            .Enum<ParentModelEnum>();
        intAction.ShouldThrowValidationError();
         
        var stringValue = "Not valid enum value";
        var stringAction = () => Validate.That(stringValue)
            .Enum<ParentModelEnum>();
        stringAction.ShouldThrow<CrossException>();
    }
}