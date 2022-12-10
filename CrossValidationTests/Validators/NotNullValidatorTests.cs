using CrossValidation.Validators;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Validators;

public class NotNullValidatorTests
{
    [Fact]
    public void NotNull()
    {
        var value = new Bogus.Faker().Lorem.Word();
        var validator = new NotNullValidator<string>(value);

        var isValid = validator.IsValid();

        isValid.ShouldBeTrue();
    }
    
    [Fact]
    public void NotNull_fails()
    {
        string? value = null;
        var validator = new NotNullValidator<string?>(value);

        var isValid = validator.IsValid();

        isValid.ShouldBeFalse();
    }
}