using CrossValidation.Errors;
using CrossValidation.Extensions;
using CrossValidation.Resources;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests;

public class ValidateTests
{
    [Fact]
    public void ValidateIs()
    {
        var action = () => Validate.Is(false);

        action.ShouldThrowValidationError();
    }

    [Fact]
    public void ValidateIs_with_error()
    {
        var expectedCode = nameof(ErrorResource.NotNull);
        var expectedDetails = "Details";
        var errorForValidation = new CrossValidationError
        {
            Code = expectedCode,
            Details = expectedDetails
        };

        var action = () => Validate.Is(false, errorForValidation);

        var error = action.ShouldThrowValidationError();
        error.Code.ShouldBe(expectedCode);
        error.Message.ShouldBe(ErrorResource.NotNull);
        error.Details.ShouldBe(expectedDetails);
    }

    [Fact]
    public void ValidateIs_with_raw_customizations()
    {
        var expectedCode = "Expected code";
        var expectedMessage = "Expected message";
        var expectedDetails = "Expected details";

        var action = () => Validate.Is(
            false,
            expectedCode,
            message: expectedMessage,
            details: expectedDetails);

        var error = action.ShouldThrowValidationError();
        error.Code.ShouldBe(expectedCode);
        error.Message.ShouldBe(expectedMessage);
        error.Details.ShouldBe(expectedDetails);
    }
}