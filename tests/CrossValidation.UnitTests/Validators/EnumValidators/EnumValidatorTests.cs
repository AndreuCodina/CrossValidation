﻿using Common.Tests;
using CrossValidation.Exceptions;
using CrossValidation.UnitTests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.UnitTests.Validators.EnumValidators;

public class EnumValidatorTests : TestBase
{
    [Fact]
    public void Validate_enumeration()
    {
        var enumValue = ParentModelEnum.Case1;
        var enumAction = () => Validate.That(enumValue)
            .Enum();
        enumAction.ShouldNotThrow();
         
        var intValue = (int)ParentModelEnum.Case1;
        var intAction = () => Validate.That(intValue)
            .Enum<ParentModelEnum>();
        intAction.ShouldNotThrow();
         
        var stringValue = nameof(ParentModelEnum.Case1);
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
        enumAction.ShouldThrow<BusinessException>();
         
        var intValue = nonExistentColorId;
        var intAction = () => Validate.That(intValue)
            .Enum<ParentModelEnum>();
        intAction.ShouldThrow<BusinessException>();
         
        var stringValue = "Not valid enum value";
        var stringAction = () => Validate.That(stringValue)
            .Enum<ParentModelEnum>();
        stringAction.ShouldThrow<BusinessException>();
    }
}