﻿using System.Net;
using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Fixtures;
using CrossValidation.Tests.TestUtils.Models;
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
        var expectedDetails = "Details";
        var errorForValidation = new CrossError
        {
            Code = expectedCode,
            Details = expectedDetails
        };

        var action = () => Validate.Must(false, errorForValidation);

        var error = action.ShouldThrowCrossError();
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
    
    private record TestError : CrossError;
}