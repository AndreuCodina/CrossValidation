using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

public class LengthRangeTests : TestBase
{
    [Fact]
    public void Return_error_when_the_validation_fails()
    {
        var value = "123";

        var action = () => Validate.That(value)
            .LengthRange(value.Length + 1, value.Length);

        var error = action.ShouldThrowCrossError<CommonCrossError.LengthRange>();
        error.Code.ShouldBe(nameof(ErrorResource.LengthRange));
    }
}