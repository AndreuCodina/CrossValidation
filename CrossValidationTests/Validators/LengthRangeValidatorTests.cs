using CrossValidation.Validators;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Validators;

public class LengthRangeValidatorTests
{
    [Fact]
    public void Validate_length()
    {
        var value = "123";
        var validator = new LengthRangeValidator(value, 3, 3);

        var isValid = validator.IsValid();

        isValid.ShouldBeTrue();
    }
    
    [Fact]
    public void Validate_length_fails()
    {
        var value = "123";
        var validator = new LengthRangeValidator(value, 4, 3);

        var isValid = validator.IsValid();

        isValid.ShouldBeFalse();
    }
}