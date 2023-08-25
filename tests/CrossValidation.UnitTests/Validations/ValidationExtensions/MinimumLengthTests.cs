using Common.Tests;
using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.UnitTests.TestUtils.Builders;
using CrossValidation.UnitTests.TestUtils.Models;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

public class MinimumLengthTests : TestBase
{
    private readonly ParentModel _model;

    public MinimumLengthTests()
    {
        _model = new ParentModelBuilder()
            .Build();
    }
    
    [Theory]
    [InlineData("word", 4)]
    [InlineData("word", 3)]
    public void Validate_value_is_not_less_than_maximum_value(string value, int minimumLength)
    {
        var action = () => Validate.Field(value)
            .MinimumLength(minimumLength);

        action.Should()
            .NotThrow();
    }
    
    [Fact]
    public void Fail_when_the_minimum_length_has_not_been_met()
    {
        var expectedMinimumLength = _model.String.Length + 1;
        
        var action = () => Validate.Field(_model.String)
            .MinimumLength(expectedMinimumLength);

        var exception = action.Should()
            .Throw<CommonException.MinimumLengthException>()
            .Which;
        exception.Code
            .Should()
            .Be(nameof(ErrorResource.MinimumLength));
        exception.MinimumLength
            .Should()
            .Be(expectedMinimumLength);
        exception.TotalLength
            .Should()
            .Be(_model.String.Length);
    }
}