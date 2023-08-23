using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

public class UniqueItemsValidatorTests : TestBase
{
    private ParentModel _model;

    public UniqueItemsValidatorTests()
    {
        _model = new ParentModelBuilder().Build();
    }

    [Fact]
    public void Validate_collection_has_unique_items()
    {
        _model = new ParentModelBuilder()
            .WithIntList(new List<int> { 1, 2 })
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
    
    public static IEnumerable<object[]> CollectionsWithUniqueItems()
    {
        yield return new object[] { new List<int> { 1, 2 } };
        yield return new object[] { new List<int>() };
    }
}