using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

public class MinimumItemsTests : TestBase
{
    private ParentModel _model;

    public MinimumItemsTests()
    {
        _model = new ParentModelBuilder().Build();
    }

    [Theory]
    [InlineData(2, true, false)]
    [InlineData(2, false, true)]
    [InlineData(1, true, false)]
    [InlineData(1, false, true)]
    public void Validate_collection_has_required_minimum_items(
        int minimumItems,
        bool isCollection,
        bool isEnumerable)
    {
        _model = new ParentModelBuilder()
            .WithIntList(new List<int> { 1, 2 })
            .Build();

        IEnumerable<int>? fieldValue = null;

        if (isCollection)
        {
            fieldValue = _model.IntList;
        }
        else if (isEnumerable)
        {
            fieldValue = _model.IntList
                .Select(x => x);
        }
        
        var action = () => Validate.Field(fieldValue!)
            .MinimumItems(minimumItems);

        action.Should()
            .NotThrow();
    }
     
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Fail_when_collection_has_not_required_minimum_items(
        bool isCollection,
        bool isEnumerable)
    {
        var expectedIntList = new List<int> { 1, 2 };
        var expectedMinimumItems = 3;
        _model = new ParentModelBuilder()
            .WithIntList(expectedIntList)
            .Build();
        
        IEnumerable<int>? fieldValue = null;

        if (isCollection)
        {
            fieldValue = _model.IntList;
        }
        else if (isEnumerable)
        {
            fieldValue = _model.IntList
                .Select(x => x);
        }
        
        var action = () => Validate.Field(fieldValue!)
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