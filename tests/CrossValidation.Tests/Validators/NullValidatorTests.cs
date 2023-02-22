using CrossValidation.Tests.TestUtils;
using CrossValidation.Validators;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validators;

public class NullValidatorTests : TestBase
{
    [Fact]
    public void Validate_null()
    {
        string? value = null;
        var validator = new NullValidator<string?>(value);

        var isValid = validator.IsValid();

        isValid.ShouldBeTrue();
    }
    
    [Fact]
    public void Validate_null_fails()
    {
        var value = new Bogus.Faker().Lorem.Word();
        var validator = new NullValidator<string>(value);

        var isValid = validator.IsValid();

        isValid.ShouldBeFalse();
    }
}