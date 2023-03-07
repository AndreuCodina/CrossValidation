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
    public void Return_last_known_validation_type_before_accumulate_operations()
    {
        var expectedMessage = "Expected message";

        var validation = Validate.That(_model.NullableInt)
            .MustAsync(_commonFixture.BeValidAsync)
            .WithMessage(expectedMessage)
            .NotNull();

        (validation is IValidValidation<int>).ShouldBeTrue();

        var action = () => validation.Run();
        var error = action.ShouldThrowCrossError<CommonCrossError.NotNull>();
        error.Message.ShouldBe(expectedMessage);
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

        var action = () => Validate.That(_model.NullableInt)
            .MustAsync(_commonFixture.BeValidAsync)
            .WithMessage(expectedMessage)
            .NotNull()
            .Must(x => x == int.MaxValue)
            .Run();

        var error = action.ShouldThrowCrossError<CommonCrossError.NotNull>();
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Not_execute_predicate_returning_error_customization()
    {
        var expectedMessage = "Expected message";

        var action = () => Validate.That(_model.NullableString)
            .WithMessage(expectedMessage)
            .MustAsync(_commonFixture.NotBeValidAsync)
            .NotNull()
            .Must(x => new CrossError(Message: x.Substring(0)))
            .Run();

        var error = action.ShouldThrowCrossError<CommonCrossError.Predicate>();
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Get_transformed_instance_after_accumulation()
    {
        var instance = Validate.That(3)
            .MustAsync(_commonFixture.BeValidAsync)
            .Transform(x => x.ToString())
            .Transform(x => x + "0")
            .Transform(int.Parse)
            .Instance();

        instance.ShouldBe(30);
    }

    [Fact]
    public void WhenNotNull_does_not_fails_with_transformed_field_value()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();
        
        var action = () => Validate.That(_model.NullableInt)
            .WhenAsync(_commonFixture.BeValidAsync)
            .WhenNotNull(x => x
                .Must(x => x == 1))
            .Transform(x => (int?)null)
            .Must(x => x is null)
            .Run();

        action.ShouldNotThrow();
    }
    
    // [Fact]
    // public void Accumulate_operations_after_async_validator()
    // {
    //     var expectedMessage = "Expected message";
    //
    //     var action = () => Validate.That(_model.IntList)
    //         .MustAsync(_commonFixture.BeValidAsync)
    //         .ForEach(x =>
    //             x.Must(x => x == int.MaxValue));
    //         .WithMessage(expectedMessage)
    //         .NotNull() // Returns a ValidValidation because we couldn't execute the validation due to we have accumulated operations
    //         .Must(x => x == int.MaxValue)
    //         .Run();
    //
    //     var error = action.ShouldThrowCrossError<CommonCrossError.NotNull>();
    //     error.Message.ShouldBe(expectedMessage);
    // }
    
    // TODO: Use a ModelValidator
    // A ModelValidator doesn't call .RunAsync in each validation, so
    // myModelValidator.RunAsync() will execute all ValidationContext.
    // How do I know when to stop and respect the ValidationMode, subModelValidations, etc. ??
}