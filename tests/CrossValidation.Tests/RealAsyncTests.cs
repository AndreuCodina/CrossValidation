using CrossValidation.Errors;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Fixtures;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests;

public class RealAsyncTests :
    TestBase,
    IClassFixture<CommonFixture>
{
    private readonly CommonFixture _commonFixture;
    private ParentModel _model;

    public RealAsyncTests(CommonFixture commonFixture)
    {
        _commonFixture = commonFixture;
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void Execute_async_validation_node()
    {
        var expectedMessage = "Expected message";

        var action = () => Validate.That(_model.Int)
            .Must(_commonFixture.BeValid)
            .WithMessage(expectedMessage)
            .MustAsync(_commonFixture.NotBeValidAsync)
            .Must(_commonFixture.NotBeValid)
            .Run();

        var error = action.ShouldThrowCrossError();
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Not_execute_accumulated_operations_when_any_not_accumulated_already_failed()
    {
        var expectedMessage = "Expected message";

        var action = () => Validate.That(_model.NullableString)
            .WithMessage(expectedMessage)
            .NotNull()
            .MustAsync(_commonFixture.BeValidAsync)
            .Run();

        var error = action.ShouldThrowCrossError<CommonCrossError.NotNull>();
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Execute_validation_after_async_validation_node()
    {
        var expectedMessage = "Expected message";

        var action = () => Validate.That(_model.Int)
            .Must(_commonFixture.BeValid)
            .MustAsync(_commonFixture.BeValidAsync)
            .WithMessage(expectedMessage)
            .Must(_commonFixture.NotBeValid)
            .Run();

        var error = action.ShouldThrowCrossError();
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Stop_execution_after_first_failed_validation_operation_accumulated()
    {
        var expectedMessage = "Expected message";

        var action = () => Validate.That(_model.NullableString)
            .MustAsync(_commonFixture.BeValidAsync)
            .WithMessage(expectedMessage)
            .NotNull() // Returns a ValidValidation because we couldn't execute the validation due to we have accumulated operations
            .Must(x => x.StartsWith("Random start"))
            .Run();

        var error = action.ShouldThrowCrossError<CommonCrossError.NotNull>();
        error.Message.ShouldBe(expectedMessage);
    }
    
    // TODO: Use a ModelValidator
    // A ModelValidator doesn't call .RunAsync in each validation, so
    // myModelValidator.RunAsync() will execute all ValidationContext.
    // How do I know when to stop and respect the ValidationMode, subModelValidations, etc. ??
}