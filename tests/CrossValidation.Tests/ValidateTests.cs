using System.Net;
using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Fixtures;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests;

public class ValidateTests :
    TestBase,
    IClassFixture<CommonFixture>
{
    private readonly CommonFixture _commonFixture;
    private ParentModel _model;

    public ValidateTests(CommonFixture commonFixture)
    {
        _commonFixture = commonFixture;
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void ValidateMust()
    {
        var action = () => Validate.Must(false);

        action.ShouldThrowCrossError();
    }

    [Fact]
    public void ValidateMust_with_error()
    {
        var expectedCode = nameof(ErrorResource.NotNull);
        var expectedMessage = ErrorResource.NotNull;
        var expectedDetails = "Expected details";
        var errorForValidation = new CrossError
        {
            Code = expectedCode,
            Details = expectedDetails
        };

        var action = () => Validate.Must(false, errorForValidation);

        var error = action.ShouldThrowCrossError();
        error.Code.ShouldBe(expectedCode);
        error.Message.ShouldBe(expectedMessage);
        error.Details.ShouldBe(expectedDetails);
    }

    [Fact]
    public void ValidateMust_with_raw_customizations()
    {
        var expectedMessage = "Expected message";
        var expectedCode = "Expected code";
        var expectedDetails = "Expected details";
        var expectedHttpStatusCode = HttpStatusCode.Created;

        var action = () => Validate.Must(
            false,
            message: expectedMessage,
            code: expectedCode,
            details: expectedDetails,
            httpStatusCode: expectedHttpStatusCode);

        var error = action.ShouldThrowCrossError();
        error.Code.ShouldBe(expectedCode);
        error.Message.ShouldBe(expectedMessage);
        error.Details.ShouldBe(expectedDetails);
        error.HttpStatusCode.ShouldBe(expectedHttpStatusCode);
    }

    [Fact]
    public void Apply_fixed_customizations()
    {
        var expectedError = new TestError();
        var expectedMessage = "Expected message";
        var expectedCode = "ExpectedCode";
        var expectedDetails = "Expected details";
        var expectedHttpStatusCode = HttpStatusCode.Created;
        var expectedFieldDisplayName = "Expected field display name";
        var action = () => Validate.That(
                _model.Int,
                error: expectedError,
                message: expectedMessage,
                code: expectedCode,
                details: expectedDetails,
                httpStatusCode: expectedHttpStatusCode,
                fieldDisplayName: expectedFieldDisplayName)
            .WithError(new CrossError())
            .WithMessage("Unexpected message")
            .WithCode("UnexpectedCode")
            .WithDetails("Unexpected details")
            .WithHttpStatusCode(HttpStatusCode.Accepted)
            .WithFieldDisplayName("Unexpected field display name")
            .Must(_commonFixture.NotBeValid);

        var error = action.ShouldThrowCrossError<TestError>();
        error.Message.ShouldBe(expectedMessage);
        error.Code.ShouldBe(expectedCode);
        error.Details.ShouldBe(expectedDetails);
        error.HttpStatusCode.ShouldBe(expectedHttpStatusCode);
        error.FieldDisplayName.ShouldBe(expectedFieldDisplayName);
    }
    
    [Fact]
    public void Apply_fixed_customizations_with_transformation()
    {
        var expectedError = new TestError();
        var expectedMessage = "Expected message";
        var expectedCode = "ExpectedCode";
        var expectedDetails = "Expected details";
        var expectedHttpStatusCode = HttpStatusCode.Created;
        var expectedFieldDisplayName = "Expected field display name";
        var action = () => Validate.That(
                _model.Int,
                error: expectedError,
                message: expectedMessage,
                code: expectedCode,
                details: expectedDetails,
                httpStatusCode: expectedHttpStatusCode,
                fieldDisplayName: expectedFieldDisplayName)
            .WithError(new CrossError())
            .WithMessage("Unexpected message")
            .WithCode("UnexpectedCode")
            .WithDetails("Unexpected details")
            .WithHttpStatusCode(HttpStatusCode.Accepted)
            .WithFieldDisplayName("Unexpected field display name")
            .Transform(x => x)
            .Must(_commonFixture.NotBeValid);

        var error = action.ShouldThrowCrossError<TestError>();
        error.Message.ShouldBe(expectedMessage);
        error.Code.ShouldBe(expectedCode);
        error.Details.ShouldBe(expectedDetails);
        error.HttpStatusCode.ShouldBe(expectedHttpStatusCode);
        error.FieldDisplayName.ShouldBe(expectedFieldDisplayName);
    }
    
    [Fact]
    public void Apply_fixed_customizations_with_collection_iteration()
    {
        var expectedError = new TestError();
        var expectedMessage = "Expected message";
        var expectedCode = "ExpectedCode";
        var expectedDetails = "Expected details";
        var expectedHttpStatusCode = HttpStatusCode.Created;
        var expectedFieldDisplayName = "Expected field display name";
        var action = () => Validate.That(
                _model.IntList,
                error: expectedError,
                message: expectedMessage,
                code: expectedCode,
                details: expectedDetails,
                httpStatusCode: expectedHttpStatusCode,
                fieldDisplayName: expectedFieldDisplayName)
            .WithError(new CrossError())
            .WithMessage("Unexpected message")
            .WithCode("UnexpectedCode")
            .WithDetails("Unexpected details")
            .WithHttpStatusCode(HttpStatusCode.Accepted)
            .WithFieldDisplayName("Unexpected field display name")
            .ForEach(x => x
                .Must(_commonFixture.NotBeValid));

        var error = action.ShouldThrowCrossError<TestError>();
        error.Message.ShouldBe(expectedMessage);
        error.Code.ShouldBe(expectedCode);
        error.Details.ShouldBe(expectedDetails);
        error.HttpStatusCode.ShouldBe(expectedHttpStatusCode);
        error.FieldDisplayName.ShouldBe(expectedFieldDisplayName);
    }
    
    // [Fact]
    // public void ValidateThat_generalizes_error()
    // {
    //     var action = () => Validate.That(_model.Int)
    //         .Must(_commonFixture.NotBeValid);
    //
    //     var error = action.ShouldThrowCrossError();
    //     error.Code.ShouldBeNull();
    //     error.Message.ShouldBeNull();
    // }
    
    [Theory]
    [InlineData(null, null, nameof(ErrorResource.General), "An error has occured")]
    [InlineData(null, "Expected message", nameof(ErrorResource.General), "Expected message")]
    [InlineData("RandomCode", null, "RandomCode", null)]
    [InlineData(nameof(ErrorResource.NotNull), null, nameof(ErrorResource.NotNull), "Must have a value")]
    public void ValidateThat_does_not_generalize_error_in_customized_code_or_message(
        string? code,
        string? message,
        string? expectedCode,
        string? expectedMessage)
    {
        var validation = Validate.That(_model.Int);

        if (code != null)
        {
            validation = validation.WithCode(code);
        }

        if (message != null)
        {
            validation = validation.WithMessage(message);
        }
        
        var action = () => validation.GreaterThan(_model.Int);

        var error = action.ShouldThrowCrossError();
        error.Code.ShouldBe(expectedCode);
        error.Message.ShouldBe(expectedMessage);
    }

    private record TestError : CrossError;
}