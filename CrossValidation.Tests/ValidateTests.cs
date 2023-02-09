using System.Net;
using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.Rules;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.Builders;
using CrossValidation.Tests.Fixtures;
using CrossValidation.Tests.Models;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests;

public class ValidateTests : IClassFixture<CommonFixture>
{
    private ParentModel _model;

    public ValidateTests()
    {
        _model = new ParentModelBuilder().Build();
    }
    
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
        var errorForValidation = new CrossError
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
        var expectedHttpStatusCode = HttpStatusCode.Created;

        var action = () => Validate.Must(
            false,
            message: expectedMessage,
            code: expectedCode,
            details: expectedDetails,
            httpStatusCode: expectedHttpStatusCode);

        var error = action.ShouldThrowValidationError();
        error.Code.ShouldBe(expectedCode);
        error.Message.ShouldBe(expectedMessage);
        error.Details.ShouldBe(expectedDetails);
        error.HttpStatusCode.ShouldBe(expectedHttpStatusCode);
    }
    
    [Fact]
    public void Mark_dsl_as_Validate()
    {
        var rule = Validate.That(_model.Int);
        rule.ShouldBeAssignableTo<IValidRule<int>>();
        ((IValidRule<int>)rule).Context.Dsl.ShouldBe(Dsl.Validate);
        
        rule = Validate.Field(_model.Int);
        rule.ShouldBeAssignableTo<IValidRule<int>>();
        ((IValidRule<int>)rule).Context.Dsl.ShouldBe(Dsl.Validate);

        var action = () => Validate.Must(false);
        action.ShouldThrowValidationError();
    }
}