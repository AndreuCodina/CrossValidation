using CrossValidation.Validators;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Validators;

public class LengthValidatorTests
{
    [Fact]
    public void Length()
    {
        var value = "123";
        var validator = new LengthValidator(value, 3, 3);

        var isValid = validator.IsValid();

        isValid.ShouldBeTrue();
    }
    
    [Fact]
    public void Length_fails()
    {
        var value = "123";
        var validator = new LengthValidator(value, 4, 3);

        var isValid = validator.IsValid();

        isValid.ShouldBeFalse();
    }
}