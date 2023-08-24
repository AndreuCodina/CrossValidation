using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.UnitTests.TestUtils;
using CrossValidation.UnitTests.TestUtils.Builders;
using CrossValidation.UnitTests.TestUtils.Models;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

public class MaximumLengthTests : TestBase
{
    private readonly ParentModel _model;

    public MaximumLengthTests()
    {
        _model = new ParentModelBuilder()
            .Build();
    }

    [Theory]
    [InlineData("word", 4)]
    [InlineData("word", 5)]
    public void Validate_value_is_not_greater_than_maximum_value(string value, int maximumLength)
    {
        var action = () => Validate.Field(value)
            .MaximumLength(maximumLength);

        action.Should()
            .NotThrow();
    }
    
    [Fact]
    public void Fail_when_the_minimum_length_has_not_been_met()
    {
        var expectedMaximumLength = _model.String.Length - 1;
        
        var action = () => Validate.Field(_model.String)
            .MaximumLength(expectedMaximumLength);

        var exception = action.Should()
            .Throw<CommonException.MaximumLengthException>()
            .Which;
        exception.Code
            .Should()
            .Be(nameof(ErrorResource.MaximumLength));
        exception.MaximumLength
            .Should()
            .Be(expectedMaximumLength);
        exception.TotalLength
            .Should()
            .Be(_model.String.Length);
    }
}