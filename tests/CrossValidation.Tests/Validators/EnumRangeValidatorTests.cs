using System;
using System.Linq;
using CrossValidation.Exceptions;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validators;

public class EnumRangeValidatorTests
{
    [Fact]
    public void Validate_enumeration_range()
    {
        var enumValue = ParentModelEnum.Red;
        var enumAction = () => Validate.That(enumValue)
            .Enum(ParentModelEnum.Blue, ParentModelEnum.Red);
        enumAction.ShouldNotThrow();
         
        var intValue = (int)ParentModelEnum.Red;
        var intAction = () => Validate.That(intValue)
            .Enum(ParentModelEnum.Blue, ParentModelEnum.Red);
        intAction.ShouldNotThrow();
         
        var stringValue = nameof(ParentModelEnum.Red);
        var stringAction = () => Validate.That(stringValue)
            .Enum(ParentModelEnum.Blue, ParentModelEnum.Red);
        stringAction.ShouldNotThrow();
    }
     
    [Fact]
    public void Validate_enumeration_range_fails()
    {
        var lastColorId = (int)Enum.GetValues<ParentModelEnum>().Last();
        var nonExistentColorId = lastColorId + 1;
         
        var enumValue = (ParentModelEnum)nonExistentColorId;
        var enumAction = () => Validate.That(enumValue)
            .Enum(ParentModelEnum.Red);
        enumAction.ShouldThrowCrossError();
        
        var validEnumValue = ParentModelEnum.Red;
        var validEnumAction = () => Validate.That(validEnumValue)
            .Enum(ParentModelEnum.Blue);
        validEnumAction.ShouldThrowCrossError();
         
        var intValue = nonExistentColorId;
        var intAction = () => Validate.That(intValue)
            .Enum(ParentModelEnum.Red);
        intAction.ShouldThrowCrossError();
         
        var stringValue = "Not valid enum value";
        var stringAction = () => Validate.That(stringValue)
            .Enum(ParentModelEnum.Red);
        stringAction.ShouldThrow<CrossException>();
    }
}