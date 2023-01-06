﻿using CrossValidation.Extensions;
using CrossValidation.Resources;
using CrossValidation.Rules;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Rules.RuleExtensions;

public class MinimumLengthTests
{
    [Fact]
    public void Return_error_when_the_validation_fails()
    {
        var value = "123";

        var action = () => Validate.That(value)
            .MinimumLength(value.Length + 1);

        var error = action.ShouldThrowValidationError<CommonCrossValidationError.MinimumLength>();
        error.Code.ShouldBe(nameof(ErrorResource.MinimumLength));
    }
}