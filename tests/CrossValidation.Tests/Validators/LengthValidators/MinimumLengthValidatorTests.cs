using CrossValidation.Tests.TestUtils;
using CrossValidation.Validators.LengthValidators;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validators.LengthValidators;

public class MinimumLengthValidatorTests : TestBase
{
    [Fact]
    public void Validate_minimum_length()
    {
        var value = "123";
        var validator = new MinimumLengthValidator(value, value.Length);

        var isValid = validator.IsValid();

        isValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_minimum_length_fails()
    {
        var value = "123";
        var validator = new MinimumLengthValidator(value, value.Length + 1);

        var isValid = validator.IsValid();

        isValid.ShouldBeFalse();
    }
}