using System;
using System.Threading.Tasks;
using CrossValidation.Errors;
using CrossValidation.Tests.TestUtils.Fixtures.Validators;
using CrossValidation.Tests.TestUtils.Models;
using Moq;

namespace CrossValidation.Tests.TestUtils.Fixtures;

public class CommonFixture
{
    public ParentModelValidator CreateParentModelValidator(Action<ParentModelValidator> validator)
    {
        var validatorMock = new Mock<ParentModelValidator>()
        {
            CallBase = true
        };
        validatorMock.Setup(x => x.CreateValidations(It.IsAny<ParentModel>()))
            .Callback(() => validator(validatorMock.Object));
        return validatorMock.Object;
    }

    public NestedModelValidator CreateNestedModelValidator(
        Action<NestedModelValidator> validator)
    {
        var validatorMock = new Mock<NestedModelValidator>()
        {
            CallBase = true
        };
        validatorMock.Setup(x => x.CreateValidations(It.IsAny<NestedModel>()))
            .Callback(() => validator(validatorMock.Object));
        return validatorMock.Object;
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
    
    public async Task<ICrossError?> ValidErrorAsync(ICrossError? error)
    {
        return await ReturnErrorTask(error);
    }
    
    private Task<bool> ReturnBoolTask(bool value)
    {
        return Task.FromResult(value);
    }
    
    private Task<ICrossError?> ReturnErrorTask(ICrossError? error)
    {
        if (error is null)
        {
            return Task.FromResult(error);
        }
        
        return Task.FromResult((ICrossError?)error);
    }
}