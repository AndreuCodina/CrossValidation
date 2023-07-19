using CrossValidation.Exceptions;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validators;

public class EnumRangeValidatorTests : TestBase
{
    [Fact]
    public void Validate_enumeration_range()
    {
        var enumValue = ParentModelEnum.Case1;
        var enumAction = () => Validate.That(enumValue)
            .Enum(ParentModelEnum.Case2, ParentModelEnum.Case1);
        enumAction.ShouldNotThrow();
         
        var intValue = (int)ParentModelEnum.Case1;
        var intAction = () => Validate.That(intValue)
            .Enum(ParentModelEnum.Case2, ParentModelEnum.Case1);
        intAction.ShouldNotThrow();
         
        var stringValue = nameof(ParentModelEnum.Case1);
        var stringAction = () => Validate.That(stringValue)
            .Enum(ParentModelEnum.Case2, ParentModelEnum.Case1);
        stringAction.ShouldNotThrow();
    }
     
    [Fact]
    public void Validate_enumeration_range_fails()
    {
        var lastColorId = (int)Enum.GetValues<ParentModelEnum>().Last();
        var nonExistentColorId = lastColorId + 1;
         
        var enumValue = (ParentModelEnum)nonExistentColorId;
        var enumAction = () => Validate.That(enumValue)
            .Enum(ParentModelEnum.Case1);
        enumAction.ShouldThrow<BusinessException>();
        
        var validEnumValue = ParentModelEnum.Case1;
        var validEnumAction = () => Validate.That(validEnumValue)
            .Enum(ParentModelEnum.Case2);
        validEnumAction.ShouldThrow<BusinessException>();
         
        var intValue = nonExistentColorId;
        var intAction = () => Validate.That(intValue)
            .Enum(ParentModelEnum.Case1);
        intAction.ShouldThrow<BusinessException>();
         
        var stringValue = "Not valid enum value";
        var stringAction = () => Validate.That(stringValue)
            .Enum(ParentModelEnum.Case1);
        stringAction.ShouldThrow<BusinessException>();
    }
}