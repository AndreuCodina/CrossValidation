using Common.Tests;
using CrossValidation.ErrorResources;
using CrossValidation.Exceptions;
using CrossValidation.UnitTests.TestUtils.Builders;
using CrossValidation.UnitTests.TestUtils.Models;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

public class MinimumItemsTests : TestBase
{
    private ParentModel _model;

    public MinimumItemsTests()
    {
        _model = new ParentModelBuilder().Build();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(1)]
    public void Validate_collection_has_required_minimum_items(int minimumItems)
    {
        _model = new ParentModelBuilder()
            .WithIntList(new List<int> { 1, 2 })
            .Build();
        
        var action = () => Validate.Field(_model.IntList)
            .MinimumItems(minimumItems);

        action.Should()
            .NotThrow();
    }
     
    [Fact]
    public void Fail_when_collection_has_not_required_minimum_items()
    {
        var expectedIntList = new List<int> { 1, 2 };
        var expectedMinimumItems = 3;
        _model = new ParentModelBuilder()
            .WithIntList(expectedIntList)
            .Build();
        
        var action = () => Validate.Field(_model.IntList)
            .MinimumItems(expectedMinimumItems);
        
        var exception = action.Should()
            .Throw<CommonException.MinimumItemsException>()
            .Which;
        exception.Code
            .Should()
            .Be(nameof(ErrorResource.MinimumItems));
        exception.MinimumItems
            .Should()
            .Be(expectedMinimumItems);
        exception.TotalItems
            .Should()
            .Be(expectedIntList.Count);
    }
}