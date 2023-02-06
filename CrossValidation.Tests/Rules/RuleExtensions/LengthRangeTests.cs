using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.Rules;
using CrossValidation.ShouldlyAssertions;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Rules.RuleExtensions;

public class LengthRangeTests
{
    [Fact]
    public void Return_error_when_the_validation_fails()
    {
        var value = "123";

        var action = () => Validate.That(value)
            .LengthRange(value.Length + 1, value.Length);

        var error = action.ShouldThrowValidationError<CommonValidationError.LengthRange>();
        error.Code.ShouldBe(nameof(ErrorResource.LengthRange));
    }
}