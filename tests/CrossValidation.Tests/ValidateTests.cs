using System.Net;
using CrossValidation.Exceptions;
using CrossValidation.Resources;
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

        action.ShouldThrow<BusinessException>();
    }

    [Fact]
    public void ValidateMust_with_exception()
    {
        var expectedCode = nameof(ErrorResource.NotNull);
        var expectedMessage = ErrorResource.NotNull;
        var expectedDetails = "Expected details";
        var exceptionForValidation = new BusinessException
        {
            Code = expectedCode,
            Details = expectedDetails
        };

        var action = () => Validate.Must(false, exceptionForValidation);

        var exception = action.ShouldThrow<BusinessException>();
        exception.Code.ShouldBe(expectedCode);
        exception.Message.ShouldBe(expectedMessage);
        exception.Details.ShouldBe(expectedDetails);
    }

    [Fact]
    public void ValidateMust_with_returned_exception_fails()
    {
        var expectedCode = nameof(ErrorResource.NotNull);
        var expectedMessage = ErrorResource.NotNull;
        var expectedDetails = "Expected details";
        var exceptionForValidation = new TestException(code: expectedCode, details: expectedDetails);

        var action = () => Validate.That(_model.NullableInt)
            .Must(_ => exceptionForValidation);

        var exception = action.ShouldThrow<TestException>();
        exception.Code.ShouldBe(expectedCode);
        exception.Message.ShouldBe(expectedMessage);
        exception.Details.ShouldBe(expectedDetails);
    }
    
    [Fact]
    public async Task ValidateMustAsync_with_returned_exception_fails()
    {
        var expectedCode = nameof(ErrorResource.NotNull);
        var expectedMessage = ErrorResource.NotNull;
        var expectedDetails = "Expected details";
        var testException = new TestException(code: expectedCode, details: expectedDetails);
        var action = () => Validate.That(_model.NullableInt)
            .MustAsync(_ => _commonFixture.ExceptionAsync(testException))
            .ValidateAsync();

        var exception = await action.ShouldThrowAsync<TestException>();
        
        exception.Code.ShouldBe(expectedCode);
        exception.Message.ShouldBe(expectedMessage);
        exception.Details.ShouldBe(expectedDetails);
    }
    
    [Fact]
    public void ValidateMust_with_null_returned_exception_not_fails()
    {
        var action = () => Validate.That(_model.NullableInt)
            .Must(_ => _commonFixture.NullableException());

        action.ShouldNotThrow();
    }
    
    [Fact]
    public void ValidateMustAsync_with_null_returned_exception_not_fails()
    {
        var action = () => Validate.That(_model.NullableInt)
            .MustAsync(_ => _commonFixture.NullExceptionAsync());

        action.ShouldNotThrow();
    }

    [Fact]
    public void ValidateMust_with_raw_customizations()
    {
        var expectedMessage = "Expected message";
        var expectedCode = "ExpectedCode";
        var expectedDetails = "Expected details";
        var expectedHttpStatusCode = HttpStatusCode.Created;

        var action = () => Validate.Must(
            false,
            message: expectedMessage,
            code: expectedCode,
            details: expectedDetails,
            statusCode: expectedHttpStatusCode);

        var exception = action.ShouldThrow<BusinessException>();
        exception.Code.ShouldBe(expectedCode);
        exception.Message.ShouldBe(expectedMessage);
        exception.Details.ShouldBe(expectedDetails);
        exception.StatusCode.ShouldBe(expectedHttpStatusCode);
    }

    [Fact]
    public void Apply_fixed_customizations()
    {
        var expectedException = new TestException();
        var expectedMessage = "Expected message";
        var expectedCode = "ExpectedCode";
        var expectedDetails = "Expected details";
        var expectedHttpStatusCode = HttpStatusCode.Created;
        var expectedFieldDisplayName = "Expected field display name";
        var action = () => Validate.That(
                _model.Int,
                exception: expectedException,
                message: expectedMessage,
                code: expectedCode,
                details: expectedDetails,
                statusCode: expectedHttpStatusCode,
                fieldDisplayName: expectedFieldDisplayName)
            .WithException(new BusinessException())
            .WithMessage("Unexpected message")
            .WithCode("UnexpectedCode")
            .WithDetails("Unexpected details")
            .WithHttpStatusCode(HttpStatusCode.Accepted)
            .WithFieldDisplayName("Unexpected field display name")
            .Must(_commonFixture.NotBeValid);

        var exception = action.ShouldThrow<TestException>();
        exception.Message.ShouldBe(expectedMessage);
        exception.Code.ShouldBe(expectedCode);
        exception.Details.ShouldBe(expectedDetails);
        exception.StatusCode.ShouldBe(expectedHttpStatusCode);
        exception.FieldDisplayName.ShouldBe(expectedFieldDisplayName);
    }
    
    [Fact]
    public void Apply_fixed_customizations_with_transformation()
    {
        var expectedException = new TestException();
        var expectedMessage = "Expected message";
        var expectedCode = "ExpectedCode";
        var expectedDetails = "Expected details";
        var expectedHttpStatusCode = HttpStatusCode.Created;
        var expectedFieldDisplayName = "Expected field display name";
        var action = () => Validate.That(
                _model.Int,
                exception: expectedException,
                message: expectedMessage,
                code: expectedCode,
                details: expectedDetails,
                statusCode: expectedHttpStatusCode,
                fieldDisplayName: expectedFieldDisplayName)
            .WithException(new BusinessException())
            .WithMessage("Unexpected message")
            .WithCode("UnexpectedCode")
            .WithDetails("Unexpected details")
            .WithHttpStatusCode(HttpStatusCode.Accepted)
            .WithFieldDisplayName("Unexpected field display name")
            .Transform(x => x)
            .Must(_commonFixture.NotBeValid);

        var exception = action.ShouldThrow<TestException>();
        exception.Message.ShouldBe(expectedMessage);
        exception.Code.ShouldBe(expectedCode);
        exception.Details.ShouldBe(expectedDetails);
        exception.StatusCode.ShouldBe(expectedHttpStatusCode);
        exception.FieldDisplayName.ShouldBe(expectedFieldDisplayName);
    }
    
    [Fact]
    public void Apply_fixed_customizations_with_collection_iteration()
    {
        var expectedException = new TestException();
        var expectedMessage = "Expected message";
        var expectedCode = "ExpectedCode";
        var expectedDetails = "Expected details";
        var expectedHttpStatusCode = HttpStatusCode.Created;
        var expectedFieldDisplayName = "Expected field display name";
        var action = () => Validate.That(
                _model.IntList,
                exception: expectedException,
                message: expectedMessage,
                code: expectedCode,
                details: expectedDetails,
                statusCode: expectedHttpStatusCode,
                fieldDisplayName: expectedFieldDisplayName)
            .WithException(new BusinessException())
            .WithMessage("Unexpected message")
            .WithCode("UnexpectedCode")
            .WithDetails("Unexpected details")
            .WithHttpStatusCode(HttpStatusCode.Accepted)
            .WithFieldDisplayName("Unexpected field display name")
            .ForEach(x => x
                .Must(_commonFixture.BeValid)
                .Must(_commonFixture.NotBeValid));

        var exception = action.ShouldThrow<TestException>();
        exception.Message.ShouldBe(expectedMessage);
        exception.Code.ShouldBe(expectedCode);
        exception.Details.ShouldBe(expectedDetails);
        exception.StatusCode.ShouldBe(expectedHttpStatusCode);
        exception.FieldDisplayName.ShouldBe(expectedFieldDisplayName);
    }

    [Theory]
    [InlineData(null, "", nameof(ErrorResource.Generic), "An error has occured")]
    [InlineData(null, "Expected message", nameof(ErrorResource.Generic), "Expected message")]
    [InlineData("RandomCode", "", "RandomCode", "")]
    [InlineData(nameof(ErrorResource.NotNull), "", nameof(ErrorResource.NotNull), "Must have a value")]
    public void ValidateThat_does_not_do_generic_the_customized_code_or_message(
        string? code,
        string message,
        string? expectedCode,
        string expectedMessage)
    {
        var validation = Validate.That(_model.Int);

        if (code != null)
        {
            validation.WithCode(code);
        }

        if (message != "")
        {
            validation.WithMessage(message);
        }
        
        var action = () => validation.GreaterThan(_model.Int);

        var exception = action.ShouldThrow<BusinessException>();
        exception.Code.ShouldBe(expectedCode);
        exception.Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public void ValidateThat_does_not_do_generic_exception_type()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();

        var action = () => Validate.That(_model.NullableInt)
            .Null();

        action.ShouldThrow<CommonException.NullException>();
    }
    
    [Fact]
    public void ValidateArgument_can_use_field_without_model()
    {
        int? field = null;
        
        var action = () => Validate.Argument(field)
            .NotNull();
        
        action.ShouldThrow<ArgumentException>();
    }
    
    [Fact]
    public void Use_index_in_field_name()
    {
        var list = new List<int?> {1, null, 2};

        var action = () => Validate.Argument(list)
            .ForEach(x => x
                .NotNull());

        var exception = action.ShouldThrow<ArgumentException>();
        exception.Message.ShouldBe($"list[1]: {ErrorResource.NotNull}");
    }
    
    [Fact]
    public void Throw_parametrized_exception()
    {
        var action = () => Validate<ArgumentException>.Argument(_model.NullableInt)
            .NotNull();
        action.ShouldThrow<ArgumentException>();
        
        action = () => Validate<FormatException>.Argument(_model.NullableInt)
            .NotNull();
        action.ShouldThrow<FormatException>();
        
        action = () => Validate<FormatException>.Argument(_model.NullableInt)
            .NotNull();
        action.ShouldThrow<FormatException>();
        
        action = () => Validate<FormatException>.Argument(_model.NullableInt)
            .NotNull();
        action.ShouldThrow<FormatException>();
    }
    
    [Fact]
    public async Task Not_execute_scope_value_when_there_are_pending_asynchronous_operations()
    {
        var action = () => Validate.That(_model.NullableIntList)
            .MustAsync(_commonFixture.BeValidAsync)
            .NotNull()
            .ForEach(x => x
                .Must(_commonFixture.ThrowException))
            .ValidateAsync();

        await action.ShouldThrowAsync<CommonException.NotNullException>();
    }
    
    [Fact]
    public async Task Not_execute_synchronous_validation_when_there_are_asynchronous_validations_pending_to_execute()
    {
        var expectedMessage = "Expected message";
        var action = () => Validate.That(_model.Int)
            .WithMessage(expectedMessage)
            .MustAsync(_commonFixture.NotBeValidAsync)
            .Must(_commonFixture.NotBeValid)
            .ValidateAsync();

        var exception = await action.ShouldThrowAsync<BusinessException>();
        
        exception.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public async Task Not_execute_accumulated_operations_when_any_not_accumulated_already_failed()
    {
        var expectedMessage = "Expected message";
        var action = () => Validate.That(_model.NullableString)
            .WithMessage(expectedMessage)
            .NotNull()
            .MustAsync(_commonFixture.BeValidAsync)
            .ValidateAsync();

        var exception = await action.ShouldThrowAsync<CommonException.NotNullException>();
        
        exception.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public async Task Execute_validation_after_async_validation_node()
    {
        var expectedMessage = "Expected message";
        var action = () => Validate.That(_model.Int)
            .Must(_commonFixture.BeValid)
            .MustAsync(_commonFixture.BeValidAsync)
            .WithMessage(expectedMessage)
            .Must(_commonFixture.NotBeValid)
            .ValidateAsync();

        var exception = await action.ShouldThrowAsync<BusinessException>();
        
        exception.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public async Task Not_execute_predicate_returning_exception_customization()
    {
        var expectedMessage = "Expected message";
        var action = () => Validate.That(_model.NullableString)
            .WithMessage(expectedMessage)
            .MustAsync(_commonFixture.NotBeValidAsync)
            .NotNull()
            .Must(x => new BusinessException(message: x.Substring(0)))
            .ValidateAsync();

        var exception = await action.ShouldThrowAsync<CommonException.PredicateException>();
        
        exception.Message.ShouldBe(expectedMessage);
    }

    private class ExceptionWithoutConstructor : Exception
    {
    }
    
    private record ValueObject(int Value)
    {
        public static ValueObject Create(int value)
        {
            Validate.Field(value)
                .WithMessage("Error message from value object")
                .GreaterThan(int.MaxValue);
            return new(value);
        }
    }
}