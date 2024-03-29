﻿using Common.Tests;
using CrossValidation.ErrorResources;
using CrossValidation.Exceptions;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

public class InclusiveLengthRangeTests : TestBase
{
    [Theory]
    [InlineData("word", 4, 4)]
    [InlineData("word", 3, 4)]
    [InlineData("word", 4, 5)]
    public void Validate_length_is_not_out_of_range(string value, int minimumLength, int maximumLength)
    {
        var action = () => Validate.Field(value)
            .InclusiveLengthRange(minimumLength, maximumLength);

        action.Should()
            .NotThrow();
    }
    
    [Theory]
    [InlineData("word", 3, 3)]
    [InlineData("word", 5, 5)]
    public void Fail_when_length_is_out_of_range(string value, int expectedMinimumLength, int expectedMaximumLength)
    {
        var action = () => Validate.Field(value)
            .InclusiveLengthRange(expectedMinimumLength, expectedMaximumLength);

        var exception = action.Should()
            .Throw<CommonException.InclusiveLengthRangeException>()
            .Which;
        exception.Code
            .Should()
            .Be(nameof(ErrorResource.InclusiveLengthRange));
        exception.MinimumLength
            .Should()
            .Be(expectedMinimumLength);
        exception.MaximumLength
            .Should()
            .Be(expectedMaximumLength);
        exception.TotalLength
            .Should()
            .Be(value.Length);
    }
}