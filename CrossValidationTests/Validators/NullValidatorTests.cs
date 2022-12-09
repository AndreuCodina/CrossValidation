using CrossValidation.Validators;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Validators;

public class NullValidatorTests
{
    [Fact]
    public void Null()
    {
        string? value = null;
        var validator = new NullValidator<string?>(value);

        var isValid = validator.IsValid();

        isValid.ShouldBeTrue();
    }
    
    [Fact]
    public void Null_fails()
    {
        var value = new Bogus.Faker().Lorem.Word();
        var validator = new NullValidator<string>(value);

        var isValid = validator.IsValid();

        isValid.ShouldBeFalse();
    }
}