using System;
using System.Threading.Tasks;
using CrossValidation.Tests.Fixtures.Validators;
using CrossValidation.Tests.Models;
using Moq;

namespace CrossValidation.Tests.Fixtures;

public class CommonFixture
{
    public ParentModelValidator CreateParentModelValidator(Action<ParentModelValidator> validator)
    {
        var validatorMock = new Mock<ParentModelValidator>()
        {
            CallBase = true
        };
        validatorMock.Setup(x => x.CreateRules(It.IsAny<ParentModel>()))
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
        validatorMock.Setup(x => x.CreateRules(It.IsAny<NestedModel>()))
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
    
    private Task<bool> ReturnBoolTask(bool value)
    {
        return Task.FromResult(value);
    }
}