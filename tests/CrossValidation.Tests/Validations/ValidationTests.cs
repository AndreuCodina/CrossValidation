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

namespace CrossValidation.Tests.Validations;

public class ValidationTests :
    TestBase,
    IClassFixture<CommonFixture>
{
    private readonly CommonFixture _commonFixture;
    private ParentModel _model;

    public ValidationTests(CommonFixture commonFixture)
    {
        _commonFixture = commonFixture;
        _model = new ParentModelBuilder().Build();
    }

    [Fact]
    public void Validate_predicate()
    {
        var action = () => Validate.That(_model.NestedModel)
            .Must(_commonFixture.NotBeValid);

        action.ShouldThrow<CommonException.PredicateException>();
    }

    [Fact]
    public void ValidateField_keeps_customizations_after_create_instance_using_ValidateField()
    {
        var action = () => Validate.Field(_model.NestedModel.Int)
            .Instance(ValueObjectFieldWithoutCustomization.Create);

        var exception = action.ShouldThrow<CommonException.GreaterThanException<int>>();
        exception.FieldName.ShouldBe("NestedModel.Int");
        exception.Code.ShouldBe(nameof(ErrorResource.GreaterThan));
        exception.Message.ShouldBe("Must be greater than 1");
    }
    
    [Theory]
    [InlineData(null, "Expected message", nameof(ErrorResource.Generic), "Expected message")]
    [InlineData("ExpectedCode", "", "ExpectedCode", "")]
    [InlineData(nameof(ErrorResource.Enum), "", nameof(ErrorResource.Enum), "Must be a valid value")]
    [InlineData("ExpectedCode", "Expected message", "ExpectedCode", "Expected message")]
    public void ValidateField_keeps_customizations_before_create_instance(
        string? code,
        string message,
        string? expectedCode,
        string expectedMessage)
    {
        var validation = Validate.Field(_model.NestedModel.Int);
        
        if (code != null)
        {
            validation.WithCode(code);
        }
        
        if (message != "")
        {
            validation.WithMessage(message);
        }

        var action = () => validation
            .WithException(new CustomExceptionWithPlaceholder(10))
            .Instance(x => ValueObjectWithCustomization.Create(x, code: null, message: ""));

        var exception = action.ShouldThrow<CustomExceptionWithPlaceholder>();
        exception.Code.ShouldBe(expectedCode);
        exception.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Keep_localized_message_before_create_instance()
    {
        var action = () => Validate.Field(_model.NestedModel.Int)
            .WithCode(nameof(ErrorResource.NotNull))
            .Instance(ValueObjectWithoutCustomization.Create);

        var exception = action.ShouldThrow<CommonException.GreaterThanException<int>>();
        exception.Code.ShouldBe(nameof(ErrorResource.NotNull));
        exception.Message.ShouldBe(ErrorResource.NotNull);
    }
    
    [Fact]
    public void Keep_message_before_create_instance()
    {
        var action = () => Validate.Field(_model.NestedModel.Int)
            .WithMessage(ErrorResource.NotNull)
            .Instance(ValueObjectWithoutCustomization.Create);

        var exception = action.ShouldThrow<CommonException.GreaterThanException<int>>();
        exception.Message.ShouldBe(ErrorResource.NotNull);
    }

    [Theory]
    [InlineData(null, "", nameof(ErrorResource.Generic), "An error has occured")]
    [InlineData(null, "Expected message", nameof(ErrorResource.Generic), "Expected message")]
    [InlineData("RandomCode", "", "RandomCode", "")]
    [InlineData(nameof(ErrorResource.NotNull), "", nameof(ErrorResource.NotNull), "Must have a value")]
    public void Keep_instance_customizations(
        string? code,
        string message,
        string? expectedCode,
        string expectedMessage)
    {
        var action = () => Validate.Field(_model.Int)
            .Instance(x => ValueObjectWithCustomization.Create(x, code, message));

        var exception = action.ShouldThrow<CommonException.GreaterThanException<int>>();
        exception.Code.ShouldBe(expectedCode);
        exception.Message.ShouldBe(expectedMessage);
        exception.Details.ShouldBe("Expected details");
        exception.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        exception.FieldDisplayName.ShouldBe("Expected field display name");
    }

    [Fact]
    public void Get_instance()
    {
        Validate.That(_model.Int)
            .Instance()
            .ShouldBe(_model.Int);

        Validate.That(_model.NullableInt)
            .Instance()
            .ShouldBe(_model.NullableInt);
    }
    
    [Fact]
    public void Get_transformed_instance()
    {
        var expectedValue = 1;
        _model = new ParentModelBuilder()
            .WithNullableInt(expectedValue)
            .Build();

        Validate.That(_model.NullableInt)
            .NotNull()
            .Instance()
            .ShouldBe(expectedValue);
        
        Validate.That(_model.Int)
            .Transform(_ => expectedValue)
            .Instance()
            .ShouldBe(expectedValue);
        
        Validate.That(nameof(ParentModelEnum.Case1))
            .Enum<ParentModelEnum>()
            .Instance()
            .ShouldBe(ParentModelEnum.Case1);
    }

    [Fact]
    public void Validator_with_conditional_execution()
    {
        var expectedMessage = "Expected message";
        
        var action = () => Validate.That(_model.NestedModel.Int)
            .When(_commonFixture.NotBeValid)
            .GreaterThan(_model.NestedModel.Int + 1)
            .When(() => true)
            .WithMessage(expectedMessage)
            .GreaterThan(_model.NestedModel.Int);

        var exception = action.ShouldThrow<BusinessException>();
        exception.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public async Task Validator_with_async_conditional_execution()
    {
        var expectedMessage = "TrueCase";
        var action = () => Validate.That(_model.NestedModel.Int)
            .WhenAsync(_commonFixture.NotBeValidAsync)
            .GreaterThan(_model.NestedModel.Int + 1)
            .WhenAsync(_commonFixture.BeValidAsync)
            .WithMessage(expectedMessage)
            .GreaterThan(_model.NestedModel.Int)
            .ValidateAsync();

        var exception = await action.ShouldThrowAsync<BusinessException>();
        exception.Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public void Must()
    {
        var action = () => Validate.That(1)
            .Must(x => x == 1);

        action.ShouldNotThrow();
    }

    [Fact]
    public void Must_fails()
    {
        var action = () => Validate.That(1)
            .Must(x => x != 1);

        action.ShouldThrow<BusinessException>();
    }
    
    [Fact]
    public void MustAsync()
    {
        var action = () => Validate.That(1)
            .MustAsync(_commonFixture.BeValidAsync);

        action.ShouldNotThrow();
    }
    
    [Fact]
    public async Task MustAsync_fails()
    {
        var action = () => Validate.That(1)
            .MustAsync(_commonFixture.NotBeValidAsync)
            .ValidateAsync();

        await action.ShouldThrowAsync<BusinessException>();
    }
    
    [Fact]
    public void Repeat_customization_applies_new_customization()
    {
        var expectedMessage = "Expected message";

        var action = () => Validate.That(_model.NullableInt)
            .WithMessage("Old message")
            .WithMessage(expectedMessage)
            .NotNull();

        var exception = action.ShouldThrow<BusinessException>();
        exception.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Repeat_exception_customization_applies_new_exception()
    {
        var action = () => Validate.That(_model.NullableInt)
            .WithException(new CommonException.NotNullException())
            .WithException(new CommonException.EnumException())
            .Must(_commonFixture.NotBeValid);

        var exception = action.ShouldThrow<CommonException.EnumException>();
        exception.Code.ShouldBe(nameof(ErrorResource.Enum));
        exception.Message.ShouldBe(ErrorResource.Enum);
    }
    
    [Fact]
    public void Validators_do_not_override_customization()
    {
        var expectedDetails = "Expected details";
        var expectedHttpStatusCode = HttpStatusCode.Created;
        var action = () => Validate.That(_model.Int)
            .WithCode(nameof(ErrorResource.NotNull))
            .WithDetails(expectedDetails)
            .WithHttpStatusCode(HttpStatusCode.Created)
            .GreaterThan(_model.Int);

        var exception = action.ShouldThrow<CommonException.GreaterThanException<int>>();
        exception.Code.ShouldBe(nameof(ErrorResource.NotNull));
        exception.Message.ShouldBe(ErrorResource.NotNull);
        exception.Details.ShouldBe(expectedDetails);
        exception.StatusCode.ShouldBe(expectedHttpStatusCode);
    }
    
    [Fact]
    public void Validators_do_not_override_exception_customization()
    {
        var expectedDetails = "Expected details";
        var action = () => Validate.That(_model.Int)
            .WithException(new ExceptionWithCustomization())
            .GreaterThan(_model.Int);

        var exception = action.ShouldThrow<ExceptionWithCustomization>();
        exception.Code.ShouldBe(nameof(ErrorResource.NotNull));
        exception.Message.ShouldBe(ErrorResource.NotNull);
        exception.Details.ShouldBe(expectedDetails);
        exception.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

#pragma warning disable CS9113 // Parameter is unread.
    private class CustomExceptionWithPlaceholder(int value) : BusinessException;
#pragma warning restore CS9113 // Parameter is unread.

    private record ValueObjectWithoutCustomization(int Value)
    {
        public static ValueObjectWithoutCustomization Create(int value)
        {
            Validate.That(value).GreaterThan(value + 1);
            return new(value);
        }
    }
    
    private record ValueObjectFieldWithoutCustomization(int Value)
    {
        public static ValueObjectWithoutCustomization Create(int value)
        {
            Validate.Field(value)
                .GreaterThan(value + 1);
            return new(value);
        }
    }

    private record ValueObjectWithCustomization(int Value)
    {
        public static ValueObjectWithCustomization Create(int value, string? code, string message)
        {
            var validation = Validate.That(value);

            if (code is not null)
            {
                validation.WithCode(code);
            }

            if (message != "")
            {
                validation.WithMessage(message);
            }
                
            validation
                .WithDetails("Expected details")
                .WithHttpStatusCode(HttpStatusCode.Accepted)
                .WithFieldDisplayName("Expected field display name")
                .GreaterThan(value + 1);
            return new(value);
        }
    }

    private class ExceptionWithCustomization() : BusinessException(
        code: nameof(ErrorResource.NotNull),
        details: "Expected details",
        statusCode: HttpStatusCode.Created);
}