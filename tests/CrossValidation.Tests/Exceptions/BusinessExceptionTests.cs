﻿using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Fixtures;
using CrossValidation.Tests.TestUtils.Fixtures.Validators;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Exceptions;

public class BusinessExceptionTests :
    TestBase,
    IClassFixture<CommonFixture>,
    IClassFixture<ValidatorFixture>
{
    private readonly CommonFixture _commonFixture;
    private readonly ValidatorFixture _validatorFixture;

    public BusinessExceptionTests(CommonFixture commonFixture, ValidatorFixture validatorFixture)
    {
        _commonFixture = commonFixture;
        _validatorFixture = validatorFixture;
    }

    [Fact]
    public void Throw_cross_exception_with_code_customizes_message()
    {
        Action action = () => throw new CustomNotNull();

        var error = action.ShouldThrowCrossError<CustomNotNull>();
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
        error.Message.ShouldBe(ErrorResource.NotNull);
    }

    private class ErrorWithAllFieldsAddedAsPlaceholders(string name, int age) : BusinessException
    {
        public override void AddParametersAsPlaceholderValues()
        {
            AddPlaceholderValue(name);
            AddPlaceholderValue(age);
        }
    }
    
    private class ErrorWithNullFields(string? name, int? age) : BusinessException
    {
        public override void AddParametersAsPlaceholderValues()
        {
            AddPlaceholderValue(name);
            AddPlaceholderValue(age);
        }
    }
    
    private class CustomNotNull() : BusinessException(code: nameof(ErrorResource.NotNull));
}