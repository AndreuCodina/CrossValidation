using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

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

        var exception = action.ShouldThrow<CommonCrossException.EmptyString>();
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

        var exception = action.ShouldThrow<CommonCrossException.EmptyCollection>();
        exception.Code.ShouldBe(nameof(ErrorResource.EmptyCollection));
    }
}