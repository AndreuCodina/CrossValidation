using CrossValidation.UnitTests.TestUtils;
using CrossValidation.Validators.NullValidators;
using Shouldly;
using Xunit;

namespace CrossValidation.UnitTests.Validators;

public class NotNullValidatorTests : TestBase
{
    [Fact]
    public void Validate_not_null()
    {
        var value = new Bogus.Faker().Lorem.Word();
        var validator = new NotNullValidator<string>(value);

        var isValid = validator.IsValid();

        isValid.ShouldBeTrue();
    }
    
    [Fact]
    public void Validate_not_null_fails()
    {
        string? value = null;
        var validator = new NotNullValidator<string?>(value);

        var isValid = validator.IsValid();

        isValid.ShouldBeFalse();
    }
}