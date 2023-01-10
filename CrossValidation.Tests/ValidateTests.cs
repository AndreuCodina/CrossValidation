using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.ShouldlyAssertions;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests;

public class ValidateTests
{
    [Fact]
    public void ValidateMust()
    {
        var action = () => Validate.Must(false);

        action.ShouldThrowValidationError();
    }

    [Fact]
    public void ValidateMust_with_error()
    {
        var expectedCode = nameof(ErrorResource.NotNull);
        var expectedDetails = "Details";
        var errorForValidation = new ValidationError
        {
            Code = expectedCode,
            Details = expectedDetails
        };

        var action = () => Validate.Must(false, errorForValidation);

        var error = action.ShouldThrowValidationError();
        error.Code.ShouldBe(expectedCode);
        error.Message.ShouldBe(ErrorResource.NotNull);
        error.Details.ShouldBe(expectedDetails);
    }

    [Fact]
    public void ValidateMust_with_raw_customizations()
    {
        var expectedMessage = "Expected message";
        var expectedCode = "Expected code";
        var expectedDetails = "Expected details";

        var action = () => Validate.Must(
            false,
            message: expectedMessage,
            code: expectedCode,
            details: expectedDetails);

        var error = action.ShouldThrowValidationError();
        error.Code.ShouldBe(expectedCode);
        error.Message.ShouldBe(expectedMessage);
        error.Details.ShouldBe(expectedDetails);
    }
}