using CrossValidation;
using CrossValidation.Extensions;
using CrossValidation.Resources;
using CrossValidation.Rules;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Rules.RuleExtensions;

public class LengthRangeTests
{
    [Fact]
    public void Return_error_when_the_validation_fails()
    {
        var value = "123";

        var action = () => Validate.That(value)
            .LengthRange(value.Length + 1, value.Length);

        var error = action.ShouldThrowValidationError<CommonCrossValidationError.LengthRange>();
        error.Code.ShouldBe(nameof(ErrorResource.LengthRange));
    }
}