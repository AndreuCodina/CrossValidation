﻿using System;
using CrossValidationTests.Fixtures.Validators;
using CrossValidationTests.Models;
using Moq;

namespace CrossValidationTests.Fixtures;

public class ModelValidatorFixture
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
}