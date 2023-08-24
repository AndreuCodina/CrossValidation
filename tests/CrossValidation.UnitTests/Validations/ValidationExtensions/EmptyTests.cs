using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.UnitTests.TestUtils;
using CrossValidation.UnitTests.TestUtils.Builders;
using CrossValidation.UnitTests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

public class EmptyTests : TestBase
{
    private ParentModel _model;

    public EmptyTests()
    {
        _model = new ParentModelBuilder()
            .Build();
    }
    
    [Fact]
    public void Not_fail_when_field_value_is_empty_string()
    {
        _model = new ParentModelBuilder()
            .WithString("")
            .Build();
        
        var action = () => Validate.Field(_model.String)
            .Empty();

        action.ShouldNotThrow();
    }
    
    [Fact]
    public void Fail_when_field_value_is_not_empty_string()
    {
        var action = () => Validate.Field(_model.String)
            .Empty();

        var exception = action.ShouldThrow<CommonException.EmptyStringException>();
        exception.Code.ShouldBe(nameof(ErrorResource.EmptyString));
    }
    
    [Fact]
    public void Not_fail_when_field_value_is_empty_collection()
    {
        _model = new ParentModelBuilder()
            .WithIntList(new())
            .Build();
        
        var action = () => Validate.Field(_model.IntList)
            .Empty();

        action.ShouldNotThrow();
    }
    
    [Fact]
    public void Fail_when_field_value_is_not_empty_collection()
    {
        var action = () => Validate.Field(_model.IntList)
            .Empty();

        var exception = action.ShouldThrow<CommonException.EmptyCollectionException>();
        exception.Code.ShouldBe(nameof(ErrorResource.EmptyCollection));
    }
}