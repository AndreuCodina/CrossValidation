using CrossValidation.Exceptions;
using CrossValidation.UnitTests.TestUtils.Fixtures.Validators;
using NSubstitute;

namespace CrossValidation.UnitTests.TestUtils.Fixtures;

public class CommonFixture
{
    public ParentModelValidator CreateParentModelValidator(Action<ParentModelValidator> validator)
    {
        var modelValidator = Substitute.ForPartsOf<ParentModelValidator>();
        modelValidator.When(x => x.CreateValidations())
            .Do(_ => validator(modelValidator));
        return modelValidator;
    }

    public NestedModelValidator CreateNestedModelValidator(
        Action<NestedModelValidator> validator)
    {
        var modelValidator = Substitute.ForPartsOf<NestedModelValidator>();
        modelValidator.When(x => x.CreateValidations())
            .Do(_ => validator(modelValidator));
        return modelValidator;
    }

    public bool BeValid<T>(T parameter)
    {
        return true;
    }
    
    public async Task<bool> BeValidAsync<T>(T parameter)
    {
        return await ReturnBoolTask(true);
    }
    
    public bool NotBeValid<T>(T parameter)
    {
        return false;
    }
    
    public async Task<bool> NotBeValidAsync<T>(T parameter)
    {
        return await ReturnBoolTask(false);
    }
    
    public BusinessException? Exception()
    {
        BusinessException? exception = new TestException();
        return exception;
    }

    public async Task<BusinessException?> ExceptionAsync(BusinessException? exception = null)
    {
        if (exception is null)
        {
            return await ReturnExceptionTask(new TestException());
        }
        
        return await ReturnExceptionTask(exception);
    }
    
    public BusinessException? NullableException()
    {
        return default;
    }
    
    public async Task<BusinessException?> NullExceptionAsync()
    {
        BusinessException? exception = null;
        return await ReturnExceptionTask(exception);
    }
    
    public bool ThrowException<T>(T parameter)
    {
        throw new Exception("Unexpected exception");
    }
    
    private Task<bool> ReturnBoolTask(bool value)
    {
        return Task.FromResult(value);
    }
    
    private Task<BusinessException?> ReturnExceptionTask(BusinessException? exception)
    {
        return Task.FromResult(exception);
    }
}