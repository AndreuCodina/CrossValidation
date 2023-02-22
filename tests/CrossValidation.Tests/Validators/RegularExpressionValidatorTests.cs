using CrossValidation.Validators;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validators;

public class RegularExpressionValidatorTests : TestBase
{
    [Fact]
    public void Validate_regular_expression()
    {
        var value = "name";
        var pattern = "[a-z]";
        var validator = new RegularExpressionValidator(value, pattern);

        var isValid = validator.IsValid();

        isValid.ShouldBeTrue();
    }
    
    [Fact]
    public void Validate_regular_expression_fails()
    {
        var value = "nAme";
        var pattern = "[a-z]";
        var validator = new RegularExpressionValidator(value, pattern);

        var isValid = validator.IsValid();

        isValid.ShouldBeTrue();
    }
}