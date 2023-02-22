﻿using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

public class MinimumLengthTests : TestBase
{
    [Fact]
    public void Return_error_when_the_validation_fails()
    {
        var value = "123";

        var action = () => Validate.That(value)
            .MinimumLength(value.Length + 1);

        var error = action.ShouldThrowCrossError<CommonCrossError.MinimumLength>();
        error.Code.ShouldBe(nameof(ErrorResource.MinimumLength));
    }
}