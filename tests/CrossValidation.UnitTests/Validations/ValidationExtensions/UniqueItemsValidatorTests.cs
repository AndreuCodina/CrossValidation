using Common.Tests;
using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.UnitTests.TestUtils.Builders;
using CrossValidation.UnitTests.TestUtils.Models;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

public class UniqueItemsValidatorTests : TestBase
{
    private ParentModel _model;

    public UniqueItemsValidatorTests()
    {
        _model = new ParentModelBuilder().Build();
    }

    [Theory]
    [MemberData(nameof(CollectionsWithUniqueItems))]
    public void Validate_collection_has_unique_items(List<int> intList)
    {
        _model = new ParentModelBuilder()
            .WithIntList(intList)
            .Build();
        
        var action = () => Validate.Field(_model.IntList)
            .UniqueItems();

        action.Should()
            .NotThrow();
    }
     
    [Fact]
    public void Fail_when_collection_has_repeated_items()
    {
        _model = new ParentModelBuilder()
            .WithIntList(new List<int> { 1, 2, 1 })
            .Build();
        
        var action = () => Validate.Field(_model.IntList)
            .UniqueItems();
        
        action.Should()
            .Throw<CommonException.UniqueItemsException>()
            .And
            .Code
            .Should()
            .Be(nameof(ErrorResource.UniqueItems));
    }
    
    public static List<object[]> CollectionsWithUniqueItems()
    {
        return new()
        {
            new object[] { new List<int> { 1, 2 } },
            new object[] { new List<int>() }
        };
    }
}