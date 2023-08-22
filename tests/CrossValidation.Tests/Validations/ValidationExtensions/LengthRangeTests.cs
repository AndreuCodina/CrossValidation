﻿using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

public class LengthRangeTests : TestBase
{
    private readonly ParentModel _model;

    public LengthRangeTests()
    {
        _model = new ParentModelBuilder().Build();
    }
    
    [Theory]
    [InlineData("word", 4, 4)]
    [InlineData("word", 3, 4)]
    [InlineData("word", 4, 5)]
    public void Validate_length_is_not_out_of_range(string value, int minimumLength, int maximumLength)
    {
        var action = () => Validate.Field(value)
            .LengthRange(minimumLength, maximumLength);

        action.Should()
            .NotThrow();
    }
    
    [Theory]
    [InlineData("word", 3, 3)]
    [InlineData("word", 5, 5)]
    public void Fail_when_length_is_out_of_range(string value, int expectedMinimumLength, int expectedMaximumLength)
    {
        var action = () => Validate.Field(value)
            .LengthRange(expectedMinimumLength, expectedMaximumLength);

        var exception = action.Should()
            .Throw<CommonException.LengthRangeException>()
            .Which;
        exception.Code
            .Should()
            .Be(nameof(ErrorResource.LengthRange));
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
    
    [Theory]
    [InlineData(-1, 10)]
    [InlineData(10, -1)]
    [InlineData(10, 1)]
    public void Fail_when_minimum_argument_preconditions_are_not_met(int minimumLength, int maximumLength)
    {
        var action = () => Validate.Field(_model.String)
            .LengthRange(minimumLength, maximumLength);

        action.Should()
            .Throw<ArgumentException>();
    }
}