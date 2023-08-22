using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

public class ExclusiveLengthRangeTests : TestBase
{
    private readonly ParentModel _model;

    public ExclusiveLengthRangeTests()
    {
        _model = new ParentModelBuilder().Build();
    }
    
    [Theory]
    [InlineData("word", 3, 5)]
    public void Validate_length_is_not_out_of_range(string value, int minimumLength, int maximumLength)
    {
        var action = () => Validate.Field(value)
            .ExclusiveLengthRange(minimumLength, maximumLength);

        action.Should()
            .NotThrow();
    }
    
    [Theory]
    [InlineData("word", 3, 3)]
    [InlineData("word", 5, 5)]
    [InlineData("word", 4, 4)]
    public void Fail_when_length_is_out_of_range(string value, int expectedMinimumLength, int expectedMaximumLength)
    {
        var action = () => Validate.Field(value)
            .ExclusiveLengthRange(expectedMinimumLength, expectedMaximumLength);

        var exception = action.Should()
            .Throw<CommonException.ExclusiveLengthRangeException>()
            .Which;
        exception.Code
            .Should()
            .Be(nameof(ErrorResource.ExclusiveLengthRange));
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