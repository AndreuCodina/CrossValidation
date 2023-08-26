using Common.Tests;
using CrossValidation.UnitTests.TestUtils.Builders;
using CrossValidation.UnitTests.TestUtils.Models;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.UnitTests.Validators.LengthRangeValidators;

public class LengthRangeValidatorTests : TestBase
{
    private readonly ParentModel _model;

    public LengthRangeValidatorTests()
    {
        _model = new ParentModelBuilder().Build();
    }
    
    [Theory]
    [InlineData("word", 4, 4)]
    [InlineData("word", 4, 5)]
    public void Validate_argument_preconditions_are_met(string value, int minimumLength, int maximumLength)
    {
        var action = () => Validate.Field(value)
            .InclusiveLengthRange(minimumLength, maximumLength);

        action.Should()
            .NotThrow();
    }
    
    [Theory]
    [InlineData(-1, 10)]
    [InlineData(10, -1)]
    [InlineData(10, 1)]
    public void Fail_when_minimum_argument_preconditions_are_not_met(int minimumLength, int maximumLength)
    {
        var action = () => Validate.Field(_model.String)
            .InclusiveLengthRange(minimumLength, maximumLength);

        action.Should()
            .Throw<ArgumentException>();
    }
}