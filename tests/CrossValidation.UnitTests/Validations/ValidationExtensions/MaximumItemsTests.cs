using Common.Tests;
using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.UnitTests.TestUtils.Builders;
using CrossValidation.UnitTests.TestUtils.Models;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

public class MaximumItemsTests : TestBase
{
    private ParentModel _model;

    public MaximumItemsTests()
    {
        _model = new ParentModelBuilder().Build();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void Validate_collection_has_required_minimum_items(int maximumItems)
    {
        _model = new ParentModelBuilder()
            .WithIntList(new List<int> { 1, 2 })
            .Build();
        
        var action = () => Validate.Field(_model.IntList)
            .MaximumItems(maximumItems);

        action.Should()
            .NotThrow();
    }
     
    [Fact]
    public void Fail_when_collection_has_not_required_minimum_items()
    {
        var expectedIntList = new List<int> { 1, 2 };
        var expectedMaximumItems = 1;
        _model = new ParentModelBuilder()
            .WithIntList(expectedIntList)
            .Build();
        
        var action = () => Validate.Field(_model.IntList)
            .MaximumItems(expectedMaximumItems);
        
        var exception = action.Should()
            .Throw<CommonException.MaximumItemsException>()
            .Which;
        exception.Code
            .Should()
            .Be(nameof(ErrorResource.MaximumItems));
        exception.MaximumItems
            .Should()
            .Be(expectedMaximumItems);
        exception.TotalItems
            .Should()
            .Be(expectedIntList.Count);
    }
}