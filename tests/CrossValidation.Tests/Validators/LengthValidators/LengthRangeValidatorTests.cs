using CrossValidation.Validators.LengthValidators;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validators.LengthValidators;

public class LengthRangeValidatorTests
{
    [Fact]
    public void Validate_length()
    {
        var value = "123";
        var validator = new LengthRangeValidator(value, value.Length, value.Length);

        var isValid = validator.IsValid();

        isValid.ShouldBeTrue();
    }
    
    [Fact]
    public void Validate_length_fails()
    {
        var value = "123";
        var validator = new LengthRangeValidator(value, value.Length + 1, value.Length);

        var isValid = validator.IsValid();

        isValid.ShouldBeFalse();
    }
}